using System;

namespace Database.Tables; 
public class TradingAccountCategory {
	private TradingAccount? _tradingAccount;

	/// <summary>
	/// 口座ID
	/// </summary>
	public int TradingAccountId {
		get;
		set;
	}

	/// <summary>
	/// 預り区分ID
	/// </summary>
	public int TradingAccountCategoryId {
		get;
		set;
	}

	/// <summary>
	/// 預り区分名
	/// </summary>
	public string TradingAccountCategoryName {
		get;
		set;
	} = null!;

	/// <summary>
	/// デフォルトフラグ
	/// </summary>
	public bool DefaultFlag {
		get;
		set;
	}

	/// <summary>
	/// 証券口座
	/// </summary>
	public TradingAccount TradingAccount {
		get {
			return this._tradingAccount ?? throw new InvalidOperationException();
		}
		set {
			this._tradingAccount = value;
		}
	}
}
