namespace Back.Models.Financial.ResponseDto;
public class GetTradingAccountListDto {
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
}