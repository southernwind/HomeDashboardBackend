
using System;
using System.Linq;
using System.Threading.Tasks;

using Back.Models.Common;
using Back.Models.Financial.RequestDto;
using Back.Models.Financial.ResponseDto;
using Back.Utils;

using Database.Tables;

using DataBase;

using HtmlAgilityPack;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using MoneyForwardViewer.DataBase.Tables;
using MoneyForwardViewer.Scraper;

using Reactive.Bindings;

namespace Back.Models.Financial {
	/// <summary>
	/// 財務データベースの操作
	/// </summary>
	public class FinancialModel {
		/// <summary>
		/// サービススコープ
		/// </summary>
		private readonly IServiceScope _scope;
		/// <summary>
		/// データベース
		/// </summary>
		private readonly HomeServerDbContext _db;
		/// <summary>
		/// ログ
		/// </summary>
		private readonly ILogger<FinancialModel> _logger;

		/// <summary>
		/// 非同期更新クラス
		/// </summary>
		private readonly Updater _updater;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="serviceScopeFactory">サービススコープファクトリー</param>
		/// <param name="db">データベース</param>
		/// <param name="logger">ロガー</param>
		/// <param name="updater">更新クラス</param>
		public FinancialModel(IServiceScopeFactory serviceScopeFactory, HomeServerDbContext db, ILogger<FinancialModel> logger, Updater updater) {
			this._db = db;
			this._scope = serviceScopeFactory.CreateScope();
			this._logger = logger;
			this._updater = updater;
		}

		/// <summary>
		/// 更新処理開始
		/// </summary>
		/// <param name="from">取得対象開始日</param>
		/// <param name="to">取得対象終了日</param>
		/// <returns>更新キー</returns>
		public int Update(DateTime from, DateTime to) {
			return this._updater.Update(async progress => {
				// 進捗率計算の分母
				var denominator = Math.Max(1, to.Ticks - from.Ticks);
				progress.Report(1);
				this._logger.LogInformation($"{from}-{to}の財務データベース更新開始");

				var db = this._scope.ServiceProvider.GetService<HomeServerDbContext>();
				if (db == null) {
					throw new Exception($"{typeof(HomeServerDbContext).FullName}取得失敗");
				}
				var setting = await Utility.GetUseSetting(db);
				var mfs = new MoneyForwardScraper(setting.MoneyForwardId, setting.MoneyForwardPassword);
				await using var tran = await db.Database.BeginTransactionAsync();

				// 資産推移
				var maCount = 0;
				await foreach (var ma in mfs.GetAssets(from, to)) {
					var assets =
						ma.GroupBy(x => new { x.Date, x.Institution, x.Category })
							.Select(x => new LockableMfAsset {
								Date = x.Key.Date,
								Institution = x.Key.Institution,
								Category = x.Key.Category,
								Amount = x.Sum(a => a.Amount),
								IsLocked = false
							}).ToArray();
					var existsRecords = db.MfAssets.Where(a => a.Date == assets.First().Date);
					var deleteAssetList = existsRecords.Where(x => !x.IsLocked);
					db.MfAssets.RemoveRange(deleteAssetList);

					await db.MfAssets.AddRangeAsync(assets.Where(x =>
						existsRecords
							.Where(er => er.IsLocked)
							.All(er => er.Institution != x.Institution || er.Category != x.Category)));
					this._logger.LogDebug($"{ma.First().Date:yyyy/MM/dd}資産推移{assets.Length}件登録");
					maCount += assets.Length;
					progress.Report(1 + ((ma.First().Date.Ticks - from.Ticks) * 89 / denominator));
				}
				this._logger.LogInformation($"資産推移 計{maCount}件登録");

				// 取引履歴
				var mtCount = 0;
				await foreach (var mt in mfs.GetTransactions(from, to)) {
					var ids = mt.Select(x => x.TransactionId).ToArray();
					var existsRecords = db.MfTransactions.Where(t => ids.Contains(t.TransactionId));
					var deleteTransactionList = existsRecords.Where(x => !x.IsLocked);
					db.MfTransactions.RemoveRange(deleteTransactionList);
					await db.MfTransactions.AddRangeAsync(
						mt.Select(x => new LockableMfTransaction(x))
							.Where(x =>
								existsRecords
									.Where(er => er.IsLocked)
									.All(er => er.TransactionId != x.TransactionId)));
					this._logger.LogInformation($"{mt.First()?.Date:yyyy/MM}取引履歴{mt.Length}件登録");
					mtCount += mt.Length;
					progress.Report(90 + ((mt.First().Date.Ticks - from.Ticks) * 9 / denominator));
				}
				this._logger.LogInformation($"取引履歴 計{mtCount}件登録");
				progress.Report(99);

				await db.SaveChangesAsync();
				this._logger.LogDebug("SaveChanges");
				await tran.CommitAsync();
				this._logger.LogDebug("Commit");

				this._logger.LogInformation($"{from}-{to}の財務データベース更新正常終了");
				progress.Report(100);
			});
		}

		/// <summary>
		/// 処理状況取得
		/// </summary>
		/// <param name="key">更新キー</param>
		/// <returns>処理状況(0～100: 進捗率 100:完了)</returns>
		public long GetUpdateStatus(int key) {
			return this._updater.GetUpdateStatus(key);
		}


		/// <summary>
		/// 資産推移取得
		/// </summary>
		/// <param name="from">取得対象開始日</param>
		/// <param name="to">取得対象終了日</param>
		/// <returns>資産推移データ</returns>
		public async Task<MfAsset[]> GetAssetsAsync(DateTime from, DateTime to) {
			var assets = await this._db.MfAssets.Where(x => from <= x.Date && to >= x.Date).ToArrayAsync();
			var ia = await this.GetInvestmentAssetsAsync(from, to);
			var investmentAssets = ia
				.InvestmentAssetProducts
				.SelectMany(x =>
					x.DailyRates.Select(dr => new { DailyRate = dr, x.CurrencyUnitId })
				).Select(x => {
					var currencyRate = x.CurrencyUnitId != 1 ? ia.CurrencyRates.Single(c => c.Id == x.CurrencyUnitId && c.Date == x.DailyRate.Date).LatestRate : 1;
					return new {
						Amount = (int)(x.DailyRate.Rate * x.DailyRate.Amount * currencyRate),
						x.DailyRate.Date,
					};
				})
				.GroupBy(x => x.Date)
				.Select(x => new MfAsset() {
					Date = x.Key,
					Amount = x.Sum(x => x.Amount),
					Category = "証券",
					Institution = "証券"
				});

			return assets.Concat(investmentAssets).OrderBy(x => x.Date).ThenBy(x => x.Category).ThenBy(x => x.Institution).ToArray();
		}


		/// <summary>
		/// 最新資産取得
		/// </summary>
		/// <param name="from">取得対象開始日</param>
		/// <param name="to">取得対象終了日</param>
		/// <returns>資産推移データ</returns>
		public async Task<MfAsset[]> GetLatestAssetAsync(DateTime from, DateTime to) {
			var assets = await this.GetAssetsAsync(from, to);
			var max = assets.Max(x => x.Date);
			return assets.Where(x => x.Date == max).ToArray();
		}

		/// <summary>
		/// 取引履歴取得
		/// </summary>
		/// <param name="from">取得対象開始日</param>
		/// <param name="to">取得対象終了日</param>
		/// <returns>取引履歴データ</returns>
		public async Task<MfTransaction[]> GetTransactionsAsync(DateTime from, DateTime to) {
			return await
				this._db
					.MfTransactions
					.Where(x => x.IsCalculateTarget)
					.Where(x => from <= x.Date && to >= x.Date)
					.ToArrayAsync();
		}

		/// <summary>
		/// 投資商品情報登録
		/// </summary>
		/// <param name="dto">DTO</param>
		public async Task RegisterInvestmentProduct(RegisterInvestmentProductRequestDto dto) {
			await this._db.InvestmentProducts.AddAsync(new InvestmentProduct {
				Name = dto.Name,
				Key = dto.Key,
				Type = dto.Type,
				Category = dto.Category,
				InvestmentCurrencyUnitId = dto.CurrencyUnitId,
				Enable = true
			});
			await this._db.SaveChangesAsync();
		}

		/// <summary>
		/// 投資商品取得量登録
		/// </summary>
		/// <param name="dto">DTO</param>
		public async Task RegisterInvestmentProductAmount(RegisterInvestmentProductRequestAmountDto dto) {
			var date = DateTime.Parse(dto.Date);
			await using var transaction = await this._db.Database.BeginTransactionAsync();
			var record = await this._db.InvestmentProductAmounts
				.FirstOrDefaultAsync(x =>
				x.InvestmentProductId == dto.InvestmentProductId && x.InvestmentProductAmountId == dto.InvestmentProductAmountId);
			if (record == null) {
				var max = this._db
					.InvestmentProductAmounts
					.Where(x => x.InvestmentProductId == dto.InvestmentProductId)
					.Max(x => (int?)x.InvestmentProductAmountId) ?? 0;
				await this._db.InvestmentProductAmounts.AddAsync(new InvestmentProductAmount {
					InvestmentProductId = dto.InvestmentProductId,
					InvestmentProductAmountId = max + 1,
					Date = date,
					Amount = dto.Amount,
					Price = dto.Price
				});
			} else {
				record.Date = date;
				record.Amount = dto.Amount;
				record.Price = dto.Price;
				this._db.InvestmentProductAmounts.Update(record);
			}

			await this._db.SaveChangesAsync();
			await transaction.CommitAsync();
		}

		public async Task<GetInvestmentProductAmountDto[]> GetInvestmentProductAmountList(int investmentProductId) {
			return await
				this
					._db
					.InvestmentProductAmounts
					.Where(x => x.InvestmentProductId == investmentProductId)
					.Select(x => new GetInvestmentProductAmountDto {
						InvestmentProductId = x.InvestmentProductId,
						InvestmentProductAmountId = x.InvestmentProductAmountId,
						Date = x.Date,
						Amount = x.Amount,
						Price = x.Price
					})
					.OrderByDescending(x => x.Date)
					.ToArrayAsync();
		}

		/// <summary>
		/// 投資商品情報一覧取得
		/// </summary>
		/// <returns>投資商品情報リスト</returns>
		public async Task<GetInvestmentProductListResponseDto[]> GetInvestmentProductList() {
			var rates =
				await this._db
					.InvestmentCurrencyUnits
					.Include(x => x.InvestmentCurrencyRates)
					.Select(x => new {
						id = x.Id,
						rate = x.InvestmentCurrencyRates.OrderByDescending(r => r.Date).First().Value
					})
					.ToArrayAsync();
			return (await
				this._db
					.InvestmentProducts
					.Include(x => x.InvestmentProductRates)
					.Include(x => x.InvestmentProductAmounts)
					.Select(x => new GetInvestmentProductListResponseDto {
						InvestmentProductId = x.InvestmentProductId,
						Name = x.Name,
						Key = x.Key,
						Type = x.Type,
						Category = x.Category,
						CurrencyUnitId = x.InvestmentCurrencyUnitId,
						Enable = x.Enable,
						Amount = x.InvestmentProductAmounts.Sum(ipa => ipa.Amount),
						AverageRate = x.InvestmentProductAmounts.Sum(ipa => ipa.Amount) == 0 ? 0 : x.InvestmentProductAmounts.Sum(ipa => ipa.Amount * ipa.Price) / x.InvestmentProductAmounts.Sum(ipa => ipa.Amount),
						LatestRate = (double?)x.InvestmentProductRates.OrderByDescending(ipr => ipr.Date).First().Value ?? 0
					})
					.ToArrayAsync()
					).OrderByDescending(x => x.Amount * x.LatestRate * rates.First(r => r.id == x.CurrencyUnitId).rate)
					.ToArray();
		}

		public async Task<GetInvestmentCurrencyUnitListResponseDto[]> GetInvestmentCurrencyUnitList() {
			return await this._db.InvestmentCurrencyUnits.Include(x => x.InvestmentCurrencyRates).Select(x => new GetInvestmentCurrencyUnitListResponseDto {
				Id = x.Id,
				Name = x.Name,
				Unit = x.Unit,
				NumberOfDecimalPoint = x.NumberOfDecimalPoint,
				LatestRate = x.InvestmentCurrencyRates.OrderByDescending(r => r.Date).First().Value
			}).ToArrayAsync();
		}

		public async Task<string[]> GetInvestmentProductTypeList() {
			return await this._db.InvestmentProducts.Select(x => x.Type).Distinct().ToArrayAsync();
		}
		public async Task<string[]> GetInvestmentProductCategoryList() {
			return await this._db.InvestmentProducts.Select(x => x.Category).Distinct().ToArrayAsync();
		}


		/// <summary>
		/// 投資資産推移取得
		/// </summary>
		/// <param name="from">取得対象開始日</param>
		/// <param name="to">取得対象終了日</param>
		/// <returns>投資資産推移データ</returns>
		public async Task<GetInvestmentAssetResponseDto> GetInvestmentAssetsAsync(DateTime from, DateTime to) {
			var products = await this._db
				.InvestmentProducts
				.Include(x => x.InvestmentProductAmounts)
				.Include(x => x.InvestmentProductRates)
				.ToArrayAsync();
			var currencyUnits = await
				this._db
					.InvestmentCurrencyRates
					.Where(x => x.InvestmentCurrencyUnitId != 1)
					.OrderByDescending(x => x.Date)
					.ToArrayAsync();
			var dates =
				Enumerable
					.Range(0, (int)(to - from).TotalDays + 1)
					.Select(x => from.AddDays(x))
					.ToArray();
			var result = new GetInvestmentAssetResponseDto {
				InvestmentAssetProducts = products.Select(product => {
					return new InvestmentAssetProduct {
						InvestmentProductId = product.InvestmentProductId,
						Name = product.Name,
						Category = product.Category,
						CurrencyUnitId = product.InvestmentCurrencyUnitId,
						DailyRates = dates.Select(date => {
							var amount =
								product.InvestmentProductAmounts
									.Where(ipa => ipa.Date <= date)
									.Sum(ipa => ipa.Amount);
							if (amount == 0) {
								return new DailyRate { Date = date };
							}
							var averageRate =
								product.InvestmentProductAmounts
									.Where(ipa => ipa.Date <= date)
									.Sum(ipa => ipa.Amount * ipa.Price);
							return new DailyRate {
								Date = date,
								Rate = product.InvestmentProductRates.Where(r => r.Date <= date).MaxByWithTies(r => r.Date).First()
									.Value,
								Amount = amount,
								AverageRate = averageRate / amount
							};
						}).ToArray()
					};
				}).Where(x => x != null!).ToArray(),
				CurrencyRates =
					dates
						.Join(currencyUnits.Select(x => x.InvestmentCurrencyUnitId).Distinct(), _ => true, _ => true, (date, currencyId) => (date, currencyId))
						.Select(g => {
							var (date, currencyId) = g;
							var currency = currencyUnits
								.Where(x => x.InvestmentCurrencyUnitId == currencyId && x.Date <= date)
								.MaxByWithTies(x => x.Date).FirstOrDefault();
							return new CurrencyUnit {
								Id = currency?.InvestmentCurrencyUnitId ?? -1,
								Date = date,
								LatestRate = currency?.Value ?? -1
							};
						})
						.Where( x=> x.Id != -1)
						.ToArray()
			};

			return result;
		}
	}
}
