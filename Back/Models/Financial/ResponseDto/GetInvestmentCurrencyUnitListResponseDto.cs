namespace Back.Models.Financial.ResponseDto; 
public class GetInvestmentCurrencyUnitListResponseDto {
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
	/// 小数点以下桁数
	/// </summary>
	public int NumberOfDecimalPoint {
		get;
		set;
	}

	/// <summary>
	/// 最新レート (1通貨単位あたりの円)
	/// </summary>
	public double LatestRate {
		get;
		set;
	}
}
