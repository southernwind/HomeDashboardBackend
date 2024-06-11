using System;

namespace Back.Models.Financial.ResponseDto {
	public class GetInvestmentAssetResponseDto {

		public InvestmentAssetProduct[] InvestmentAssetProducts {
			get;
			set;
		} = null!;
	}

	public class InvestmentAssetProduct {
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
		/// カテゴリ
		/// </summary>
		public string Category {
			get;
			set;
		} = null!;

		/// <summary>
		/// 通貨ID
		/// </summary>
		public int CurrencyUnitId {
			get;
			set;
		}
		/// <summary>
		/// 投資資産リスト
		/// </summary>
		public DailyRate[] DailyRates {
			get;
			set;
		} = null!;
	}

	public class DailyRate {
		public DateTime Date {
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
		/// 最新レート (1通貨単位あたりの円)
		/// </summary>
		public double CurrencyRate {
			get;
			set;
		}
	}
}
