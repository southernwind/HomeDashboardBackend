
using System;
using System.Linq;
using System.Threading.Tasks;

using Back.Models.Common;
using Back.Models.Financial.RequestDto;
using Back.Models.Financial.ResponseDto;

using Database.Tables;

using DataBase;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
			db.Database.EnsureCreated();
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
					return new {
						Amount = (int)(x.DailyRate.Rate * x.DailyRate.Amount * x.DailyRate.CurrencyRate),
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
					.OrderBy(x => x.Date)
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
					TradingAccountId = dto.TradingAccountId,
					TradingAccountCategoryId = dto.TradingAccountCategoryId,
					Date = date,
					Amount = dto.Amount,
					Price = dto.Price
				});
			} else {
				record.Date = date;
				record.Amount = dto.Amount;
				record.Price = dto.Price;
				record.TradingAccountId = dto.TradingAccountId;
				record.TradingAccountCategoryId = dto.TradingAccountCategoryId;
				this._db.InvestmentProductAmounts.Update(record);
			}

			await this._db.SaveChangesAsync();
			await transaction.CommitAsync();
		}

		/// <summary>
		/// 投資商品詳細取得
		/// </summary>
		/// <param name="investmentProductId"></param>
		/// <returns></returns>
		public async Task<GetInvestmentProductDetailDto> GetInvestmentProductDetailAsync(int investmentProductId) {
			var rates =
				await this._db
					.InvestmentCurrencyUnits
					.Include(x => x.InvestmentCurrencyRates)
					.Select(x => new {
						id = x.Id,
						rate = x.InvestmentCurrencyRates.OrderByDescending(r => r.Date).First().Value
					})
					.ToArrayAsync();
			 var result = await
				this._db
					.InvestmentProducts
					.Include(x => x.InvestmentProductRates)
					.Include(x => x.InvestmentProductAmounts)
					.Where(x => x.InvestmentProductId == investmentProductId)
					.Select(x => new GetInvestmentProductDetailDto {
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
					.FirstAsync();

			var latestRate = (await this._db.InvestmentProductRates.Where(x => x.InvestmentProductId == investmentProductId).ToArrayAsync()).MaxBy(x => x.Date)?.Value ?? 0;
			result.InvestmentProductRateList = await this._db.InvestmentProductRates.Where(x => x.InvestmentProductId == investmentProductId).Select(x => new GetInvestmentProductDetailDto.InvestmentProductRate {
				Date = x.Date,
				Rate = x.Value
			}).OrderBy(x => x.Date).ToArrayAsync();
			result.InvestmentProductAmountList = (await
				this
					._db
					.InvestmentProductAmounts
					.Include(x => x.TradingAccount)
					.Join(this._db.TradingAccountCategories, x => new { x.TradingAccountId, x.TradingAccountCategoryId }, x => new { x.TradingAccountId, x.TradingAccountCategoryId }, (Amounts, x) => new { Amounts, x.TradingAccountCategoryName })
					.Where(x => x.Amounts.InvestmentProductId == investmentProductId)
					.OrderByDescending(x => x.Amounts.Date)
					.ToArrayAsync())
					.Select(x => new GetInvestmentProductDetailDto.InvestmentProductAmount {
						InvestmentProductId = x.Amounts.InvestmentProductId,
						InvestmentProductAmountId = x.Amounts.InvestmentProductAmountId,
						Date = x.Amounts.Date,
						Amount = x.Amounts.Amount,
						Price = x.Amounts.Price,
						LatestRate = latestRate,
						TradingAccountName = x.Amounts.TradingAccount.Name,
						TradingAccountLogo = x.Amounts.TradingAccount.Logo,
						TradingAccountCategoryName = x.TradingAccountCategoryName
					})
					.ToArray();
			return result;
		}


		/// <summary>
		/// 口座情報取得
		/// </summary>
		/// <param name="tradingAccountId"></param>
		/// <returns></returns>
		public async Task<GetTradingAccountDetailDto> GetTradingAccountDetailAsync(int tradingAccountId) {
			var amountList = await this._db
					.InvestmentProductAmounts
					.Include(x => x.InvestmentProduct)
					.ThenInclude(x => x.InvestmentProductRates)
					.Join(this._db.TradingAccountCategories, x => new { x.TradingAccountId, x.TradingAccountCategoryId }, x => new { x.TradingAccountId, x.TradingAccountCategoryId }, (Amounts, x) => new { Amounts, x.TradingAccountCategoryName })
					.Where(x => x.Amounts.TradingAccountId == tradingAccountId)
					.Select(x => new GetTradingAccountDetailDto.TradingAccountDetailAmount {
						TradingAccountCategoryName = x.TradingAccountCategoryName,
						InvestmentProductId = x.Amounts.InvestmentProductId,
						InvestmentProductName = x.Amounts.InvestmentProduct.Name,
						InvestmentProductAmountId = x.Amounts.InvestmentProductAmountId,
						CurrencyUnitId = x.Amounts.InvestmentProduct.InvestmentCurrencyUnitId,
						Date = x.Amounts.Date,
						Amount = x.Amounts.Amount,
						Price = x.Amounts.Price,
						LatestRate = x.Amounts.InvestmentProduct.InvestmentProductRates.OrderByDescending(x => x.Date).First().Value
					})
					.OrderByDescending(x => x.Date)
					.ToArrayAsync();

			var amountListSummaryWithCategoryListSource = await this._db
					.InvestmentProducts
					.Include(x => x.InvestmentProductRates)
					.Include(x => x.InvestmentProductAmounts)
					.Select(x =>
						new {
							Summary = new GetTradingAccountDetailDto.TradingAccountDetailAmountSummary {
								InvestmentProductId = x.InvestmentProductId,
								Name = x.Name,
								Key = x.Key,
								Type = x.Type,
								Category = x.Category,
								CurrencyUnitId = x.InvestmentCurrencyUnitId,
								Enable = x.Enable,
								Amount = x.InvestmentProductAmounts.Where(x => x.TradingAccountId == tradingAccountId).Sum(ipa => ipa.Amount),
								AverageRate = x.InvestmentProductAmounts.Where(x => x.TradingAccountId == tradingAccountId).Sum(ipa => ipa.Amount) == 0 ? 0 : x.InvestmentProductAmounts.Where(x => x.TradingAccountId == tradingAccountId).Sum(ipa => ipa.Amount * ipa.Price) / x.InvestmentProductAmounts.Where(x => x.TradingAccountId == tradingAccountId).Sum(ipa => ipa.Amount),
								LatestRate = (double?)x.InvestmentProductRates.OrderByDescending(ipr => ipr.Date).First().Value ?? 0,
							},
							CategoryListSource = x.InvestmentProductAmounts
							.Where(x => x.TradingAccountId == tradingAccountId)
							.ToArray()
						})
					.Where(x => x.Summary.Amount != 0)
					.ToArrayAsync();
			var categoryList = await this._db.TradingAccountCategories.Where(x => x.TradingAccountId == tradingAccountId).Select(x => new {
				x.TradingAccountCategoryId,
				x.TradingAccountCategoryName
			}).ToArrayAsync();

			foreach (var summary in amountListSummaryWithCategoryListSource) {
				summary.Summary.TradingAccountCategoryDetailAmountList = summary.CategoryListSource
					.GroupBy(x => x.TradingAccountCategoryId)
					.Select(x => new GetTradingAccountDetailDto.TradingAccountDetailAmountSummary.TradingAccountCategoryDetailAmount {
						TradingAccountCategoryName = categoryList.Single(c => c.TradingAccountCategoryId == x.Key).TradingAccountCategoryName,
						Amount = x.Where(ipa => ipa.TradingAccountId == tradingAccountId && ipa.TradingAccountCategoryId == x.Key).Sum(ipa => ipa.Amount),
						AverageRate = x.Where(
							ipa => ipa.TradingAccountId == tradingAccountId && ipa.TradingAccountCategoryId == x.Key).Sum(ipa => ipa.Amount) == 0 ?
							0 :
							x.Where(ipa => ipa.TradingAccountId == tradingAccountId && ipa.TradingAccountCategoryId == x.Key).Sum(ipa => ipa.Amount * ipa.Price) / x.Where(ipa => ipa.TradingAccountId == tradingAccountId && ipa.TradingAccountCategoryId == x.Key).Sum(ipa => ipa.Amount)
					}).ToArray();
			}
			var amountListSummary = amountListSummaryWithCategoryListSource.Select(x => x.Summary);

			var account = await this._db.TradingAccounts.SingleAsync(x => x.TradingAccountId == tradingAccountId);

			var rates =
				await this._db
					.InvestmentCurrencyUnits
					.Include(x => x.InvestmentCurrencyRates)
					.Select(x => new {
						id = x.Id,
						rate = x.InvestmentCurrencyRates.OrderByDescending(r => r.Date).First().Value
					})
					.ToArrayAsync();

			return new GetTradingAccountDetailDto() {
				TradingAccountName = account.Name,
				TradingAccountLogo = account.Logo,
				TradingAccountDetailAmountList = amountList,
				TradingAccountDetailAmountSummaryList = amountListSummary.OrderByDescending(x => x.Amount * x.LatestRate * rates.First(r => r.id == x.CurrencyUnitId).rate).ToArray()
			};
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

		/// <summary>
		/// 証券口座リスト取得
		/// </summary>
		/// <param name="investmentProductId"></param>
		/// <returns></returns>
		public async Task<GetTradingAccountListDto[]> GetTradingAccountListAsync() {
			return await
				this
					._db
					.TradingAccounts
					.Include(x => x.TradingAccountCategories)
					.OrderBy(x => x.TradingAccountId)
					.Select(x => new GetTradingAccountListDto {
						TradingAccountId = x.TradingAccountId,
						Name = x.Name,
						Logo = x.Logo,
						TradingAccountCategories = x.TradingAccountCategories.Select(x => new GetTradingAccountListDto.TradingAccountCategory() {
							TradingAccountCategoryId = x.TradingAccountCategoryId,
							TradingAccountCategoryName = x.TradingAccountCategoryName,
							DefaultFlag = x.DefaultFlag
						}).ToArray()
					})
					.ToArrayAsync();
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
			var result = new GetInvestmentAssetResponseDto {
				InvestmentAssetProducts = await this._db
					.InvestmentProducts
					.Include(x => x.DailyAssetProgresses.Where(x => x.Date >= from && x.Date <= to))
					.Select(x =>
						new InvestmentAssetProduct {
							InvestmentProductId = x.InvestmentProductId,
							Name = x.Name,
							Category = x.Category,
							CurrencyUnitId = x.InvestmentCurrencyUnitId,
							DailyRates = x.DailyAssetProgresses.Select(y => new DailyRate {
								Date = y.Date,
								Rate = y.Rate,
								Amount = y.Amount,
								AverageRate = y.AverageRate,
								CurrencyRate = y.CurrencyRate
							}).ToArray()
						}).ToArrayAsync()
			};
			return result;
		}
	}
}
