namespace Back.Models.Financial.RequestDto; 
/// <summary>
/// 投資商品取得量登録リクエストDTO
/// </summary>
public class RegisterInvestmentProductRequestAmountDto {
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
	/// 取得日
	/// </summary>
	public string Date {
		get;
		set;
	} = null!;

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
}
