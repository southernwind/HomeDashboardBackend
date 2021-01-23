using System;
using System.Collections.Generic;

namespace Database.Tables {
	public class InvestmentCurrencyUnit {
		private ICollection<InvestmentProduct>? _investmentProducts;
		private ICollection<InvestmentCurrencyRate>? _investmentCurrencyRates;

		public int Id {
			get;
			set;
		}

		/// <summary>
		/// 名前
		/// </summary>
		public string Name {
			get;
			set;
		} = null!;

		/// <summary>
		/// 単位
		/// </summary>
		public string Unit {
			get;
			set;
		} = null!;

		/// <summary>
		/// データ取得に必要なキー情報
		/// </summary>
		public string? Key {
			get;
			set;
		}

		/// <summary>
		/// 小数点以下桁数
		/// </summary>
		public int NumberOfDecimalPoint {
			get;
			set;
		}

		public virtual ICollection<InvestmentProduct> InvestmentProducts {
			get {
				return this._investmentProducts ?? throw new InvalidOperationException();
			}
			set {
				this._investmentProducts = value;
			}
		}

		public virtual ICollection<InvestmentCurrencyRate> InvestmentCurrencyRates {
			get {
				return this._investmentCurrencyRates ?? throw new InvalidOperationException();
			}
			set {
				this._investmentCurrencyRates = value;
			}
		}
	}
}
