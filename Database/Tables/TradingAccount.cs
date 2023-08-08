using System;
using System.Collections.Generic;

namespace Database.Tables; 
public class TradingAccount {
	private ICollection<InvestmentProductAmount>? _investmentProductAmounts;

	/// <summary>
	/// 口座ID
	/// </summary>
	public int TradingAccountId {
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
	/// ロゴ
	/// </summary>
	public string Logo {
		get;
		set;
	} = null!;

	public virtual ICollection<InvestmentProductAmount> InvestmentProductAmounts {
		get {
			return this._investmentProductAmounts ?? throw new InvalidOperationException();
		}
		set {
			this._investmentProductAmounts = value;
		}
	}
}
