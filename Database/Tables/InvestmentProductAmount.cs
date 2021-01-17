using System;

namespace Database.Tables {
	public class InvestmentProductAmount {
		private InvestmentProduct? _investmentProduct;

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

		public InvestmentProduct InvestmentProduct {
			get {
				return this._investmentProduct ?? throw new InvalidOperationException();
			}
			set {
				this._investmentProduct = value;
			}
		}
	}
}
