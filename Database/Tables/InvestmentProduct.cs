using System;
using System.Collections.Generic;

namespace Database.Tables {
	public class InvestmentProduct {
		private ICollection<InvestmentProductAmount>? _investmentProductAmounts;
		private ICollection<InvestmentProductRate>? _investmentProductRates;

		/// <summary>
		/// 投資商品ID
		/// </summary>
		public int InvestmentProductId {
			get;
			set;
		}

		/// <summary>
		/// 投資商品名
		/// </summary>
		public string Name {
			get;
			set;
		} = null!;


		/// <summary>
		/// データ取得タイプ
		/// </summary>
		public string Type {
			get;
			set;
		} = null!;

		/// <summary>
		/// データ取得に必要なキー情報
		/// </summary>
		public string Key {
			get;
			set;
		} = null!;

		/// <summary>
		/// 有効
		/// </summary>
		public bool Enable {
			get;
			set;
		}

		public virtual ICollection<InvestmentProductAmount> InvestmentProductAmounts {
			get {
				return this._investmentProductAmounts ?? throw new InvalidOperationException();
			}
			set {
				this._investmentProductAmounts = value;
			}
		}

		public virtual ICollection<InvestmentProductRate> InvestmentProductRates {
			get {
				return this._investmentProductRates ?? throw new InvalidOperationException();
			}
			set {
				this._investmentProductRates = value;
			}
		}
	}
}
