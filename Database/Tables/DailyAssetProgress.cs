using System;

namespace Database.Tables;
public class DailyAssetProgress {
	private InvestmentProduct? _investmentProduct;

	/// <summary>
	/// 日
	/// </summary>
	public DateOnly Date {

		get;
		set;
	}

	/// <summary>
	/// 投資商品ID
	/// </summary>
	public int InvestmentProductId {

		get;
		set;
	}

	/// <summary>
	/// 当日最終レート
	/// </summary>
	public double Rate {
		get;
		set;
	}

	/// <summary>
	/// 当日所有数
	/// </summary>
	public double Amount {
		get;
		set;
	}

	/// <summary>
	/// 当日平均取得価格
	/// </summary>
	public double AverageRate {
		get;
		set;
	}

	/// <summary>
	/// 当日為替レート (1通貨単位あたりの円)
	/// </summary>
	public double CurrencyRate {
		get;
		set;
	}

	public InvestmentProduct InvestmentProduct {
		get {
			return this._investmentProduct ?? throw new InvalidOperationException();
		}
		set {
			this._investmentProduct = value;
		}
	}
}
