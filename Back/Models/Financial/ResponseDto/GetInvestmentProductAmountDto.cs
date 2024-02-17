using System;

namespace Back.Models.Financial.ResponseDto {
	public class GetInvestmentProductAmountDto {
		/// <summary>
		/// 投資商品ID
		/// </summary>
		public int InvestmentProductId {
			get;
			set;
		}

		/// <summary>
		/// 投資商品取得量ID
		/// </summary>
		public int InvestmentProductAmountId {
			get;
			set;
		}

		/// <summary>
		/// 取得日
		/// </summary>
		public DateTime Date {
			get;
			set;
		}

		/// <summary>
		/// 取得量
		/// </summary>
		public double Amount {
			get;
			set;
		}

		/// <summary>
		/// 取得単価
		/// </summary>
		public double Price {
			get;
			set;
		}

		/// <summary>
		/// 証券口座名
		/// </summary>
		public string TradingAccountName {
			get;
			set;
		} = null!;

		/// <summary>
		/// 証券口座ロゴ
		/// </summary>
		public string TradingAccountLogo {
			get;
			set;
		} = null!;

		/// <summary>
		/// 預り区分名
		/// </summary>
		public string TradingAccountCategoryName {
			get;
			set;
		} = null!;
	}
}