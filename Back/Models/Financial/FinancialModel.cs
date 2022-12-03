
using System;
using System.Linq;
using System.Threading.Tasks;

using Back.Models.Common;
using Back.Models.Financial.RequestDto;
using Back.Models.Financial.ResponseDto;
using Back.Utils;

using Database.Tables;

using DataBase;

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
			return 0;
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
			return await this._db.MfAssets.Where(x => from <= x.Date && to >= x.Date).ToArrayAsync();
		}


		/// <summary>
		/// 最新資産取得
		/// </summary>
		/// <param name="from">取得対象開始日</param>
		/// <param name="to">取得対象終了日</param>
		/// <returns>資産推移データ</returns>
		public async Task<MfAsset[]> GetLatestAssetAsync(DateTime from, DateTime to) {
			var max = await this._db.MfAssets.Where(x => from <= x.Date && to >= x.Date).MaxAsync(x => x.Date);
			return await this._db.MfAssets.Where(x => x.Date == max).ToArrayAsync();
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
								Rate = product.InvestmentProductRates.Where(r => r.Date <= date).MaxBy(r => r.Date).First()
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
								.MaxBy(x => x.Date).First();
							return new CurrencyUnit {
								Id = currency.InvestmentCurrencyUnitId,
								Date = date,
								LatestRate = currency.Value
							};
						}).ToArray()
			};

			return result;
		}
	}
}
