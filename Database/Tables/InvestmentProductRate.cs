using System;

namespace Database.Tables; 
public class InvestmentProductRate {
	private InvestmentProduct? _investmentProduct;

	/// <summary>
	/// 投資商品ID
	/// </summary>
	public int InvestmentProductId {
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

	public InvestmentProduct InvestmentProduct {
		get {
			return this._investmentProduct ?? throw new InvalidOperationException();
		}
		set {
			this._investmentProduct = value;
		}
	}
}
