using System;

namespace Database.Tables {
	public class InvestmentCurrencyRate {
		private InvestmentCurrencyUnit? _investmentCurrencyUnit;

		public int InvestmentCurrencyUnitId {
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

		public virtual InvestmentCurrencyUnit InvestmentCurrencyUnit {
			get {
				return this._investmentCurrencyUnit ?? throw new InvalidOperationException();
			}
			set {
				this._investmentCurrencyUnit = value;
			}
		}
	}
}
