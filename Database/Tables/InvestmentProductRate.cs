using System;

namespace Database.Tables {
	public class InvestmentProductRate {
		/// <summary>
		/// 投資商品ID
		/// </summary>
		public int InvestmentProductId {
			get;
			set;
		}

		public DateTime Date {
			get;
			set;
		}

		public double Value {
			get;
			set;
		}
	}
}
