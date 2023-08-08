using System;
using System.Net.Mime;
using System.Threading.Tasks;

using Back.Models.Financial;
using Back.Models.Financial.RequestDto;

using Microsoft.AspNetCore.Mvc;

namespace Back.Controllers {
	[ApiController]
	[Produces(MediaTypeNames.Application.Json)]
	[Route("api/financial-api/[action]")]
	public class FinancialApiController : ControllerBase {
		private readonly FinancialModel _financial;
		public FinancialApiController(FinancialModel financial) {
			this._financial = financial;
		}

		/// <summary>
		/// 財務データベースの更新リクエスト(FromTo指定)API
		/// </summary>
		/// <param name="term">更新期間</param>
		/// <returns>更新キー</returns>
		[HttpPost]
		[ActionName("post-update-by-term-request")]
		public JsonResult PostUpdateByTermRequest([FromBody] Term term) {
			var fromDate = term.GetFrom();
			var toDate = term.GetTo();
			return new JsonResult(new {
				key = this._financial.Update(fromDate, toDate)
			});
		}

		/// <summary>
		/// 財務データベースの更新リクエスト
		/// </summary>
		/// <param name="span">更新期間</param>API
		/// <returns>更新キー</returns>
		[HttpPost]
		[ActionName("post-update-by-span-request")]
		public JsonResult PostUpdateBySpanRequest([FromBody] Span span) {
			if (!(span.Days is { } days)) {
				throw new ArgumentException();
			}
			var to = DateTime.Now.Date;
			var from = to.AddDays(-days);
			return new JsonResult(new {
				key = this._financial.Update(from, to)
			});
		}

		/// <summary>
		/// 処理状況取得API
		/// </summary>
		/// <param name="key">更新キー</param>
		/// <returns>処理状況(0～100: 進捗率 101:完了)</returns>
		[HttpGet]
		[ActionName("get-update-status")]
		public JsonResult GetUpdateStatus(int key) {
			return new JsonResult(new {
				progress = this._financial.GetUpdateStatus(key)
			});
		}

		/// <summary>
		/// 資産推移取得API
		/// </summary>
		/// <param name="from">開始日</param>
		/// <param name="to">終了日</param>
		/// <returns>資産推移データ</returns>
		[HttpGet]
		[ActionName("get-assets")]
		public async Task<JsonResult> GetAssetsAsync(string from, string to) {
			var fromDate = DateTime.Parse(from);
			var toDate = DateTime.Parse(to);
			return new JsonResult(await this._financial.GetAssetsAsync(fromDate, toDate));
		}

		/// <summary>
		/// 最新資産取得API
		/// </summary>
		/// <param name="from">開始日</param>
		/// <param name="to">終了日</param>
		/// <returns>資産推移データ</returns>
		[HttpGet]
		[ActionName("get-latest-asset")]
		public async Task<JsonResult> GetLatestAssetAsync(string from, string to) {
			var fromDate = DateTime.Parse(from);
			var toDate = DateTime.Parse(to);
			return new JsonResult(await this._financial.GetLatestAssetAsync(fromDate, toDate));
		}

		/// <summary>
		/// 取引履歴取得API
		/// </summary>
		/// <param name="from">開始日</param>
		/// <param name="to">終了日</param>
		/// <returns>取引履歴データ</returns>
		[HttpGet]
		[ActionName("get-transactions")]
		public async Task<JsonResult> GetTransactionsAsync(string from, string to) {
			var fromDate = DateTime.Parse(from);
			var toDate = DateTime.Parse(to);
			return new JsonResult(await this._financial.GetTransactionsAsync(fromDate, toDate));
		}

		/// <summary>
		/// 投資商品情報登録
		/// </summary>
		[HttpPost]
		[ActionName("post-register-investment-product")]
		public async Task<JsonResult> PostRegisterInvestmentProduct([FromBody] RegisterInvestmentProductRequestDto dto) {
			await this._financial.RegisterInvestmentProduct(dto);
			return new JsonResult(new {
				Result = true
			});
		}

		/// <summary>
		/// 投資商品取得量登録
		/// </summary>
		[HttpPost]
		[ActionName("post-register-investment-product-amount")]
		public async Task<JsonResult> PostRegisterInvestmentProductAmount([FromBody] RegisterInvestmentProductRequestAmountDto dto) {
			await this._financial.RegisterInvestmentProductAmount(dto);
			return new JsonResult(new {
				Result = true
			});
		}

		/// <summary>
		/// 投資商品取得量一覧取得
		/// </summary>
		[HttpGet]
		[ActionName("get-investment-product-amount-list")]
		public async Task<JsonResult> GetInvestmentProductAmountList(int investmentProductId) {
			return new(await this._financial.GetInvestmentProductAmountList(investmentProductId));
		}

		/// <summary>
		/// 投資商品情報一覧取得
		/// </summary>
		[HttpGet]
		[ActionName("get-investment-product-list")]
		public async Task<JsonResult> GetInvestmentProductList() {
			return new(await this._financial.GetInvestmentProductList());
		}

		/// <summary>
		/// 通貨単位一覧取得
		/// </summary>
		[HttpGet]
		[ActionName("get-investment-currency-unit-list")]
		public async Task<JsonResult> GetInvestmentCurrencyUnitList() {
			return new(await this._financial.GetInvestmentCurrencyUnitList());
		}
		/// <summary>
		/// 投資商品タイプ一覧取得
		/// </summary>
		[HttpGet]
		[ActionName("get-investment-product-type-list")]
		public async Task<JsonResult> GetInvestmentProductTypeList() {
			return new(await this._financial.GetInvestmentProductTypeList());
		}

		/// <summary>
		/// 投資商品カテゴリー一覧取得
		/// </summary>
		[HttpGet]
		[ActionName("get-investment-product-category-list")]
		public async Task<JsonResult> GetInvestmentProductCategoryList() {
			return new(await this._financial.GetInvestmentProductCategoryList());
		}

		/// <summary>
		/// 投資資産推移取得
		/// </summary>
		/// <param name="from">開始日</param>
		/// <param name="to">終了日</param>
		/// <returns>投資データ</returns>
		[HttpGet]
		[ActionName("get-investment-assets")]
		public async Task<JsonResult> GetInvestmentAssetsAsync(string from, string to) {
			var fromDate = DateTime.Parse(from);
			var toDate = DateTime.Parse(to);
			return new JsonResult(await this._financial.GetInvestmentAssetsAsync(fromDate, toDate));
		}

		/// <summary>
		/// 投資資産推移取得
		/// </summary>
		/// <param name="from">開始日</param>
		/// <param name="to">終了日</param>
		/// <returns>投資データ</returns>
		[HttpGet]
		[ActionName("get-trading-account-list")]
		public async Task<JsonResult> GetTradingAccountListAsync() {
			return new JsonResult(await this._financial.GetTradingAccountListAsync());
		}

	}
}