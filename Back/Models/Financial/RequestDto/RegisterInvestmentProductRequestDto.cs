namespace Back.Models.Financial.RequestDto; 
/// <summary>
/// 投資商品登録リクエストDTO
/// </summary>
public class RegisterInvestmentProductRequestDto {
	/// <summary>
	/// 投資商品名
	/// </summary>
	public string Name {
		get;
		set;
	} = null!;

	/// <summary>
	/// 取得タイプ
	/// </summary>
	public string Type {
		get;
		set;
	} = null!;

	/// <summary>
	/// 投資商品カテゴリー
	/// </summary>
	public string Category {
		get;
		set;
	} = null!;

	/// <summary>
	/// 取得のためのキー情報
	/// </summary>
	public string Key {
		get;
		set;
	} = null!;

	/// <summary>
	/// 通貨単位ID
	/// </summary>
	public int CurrencyUnitId {
		get;
		set;
	}

}
