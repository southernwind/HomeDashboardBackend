namespace Back.Models.Financial.ResponseDto {
	public class GetInvestmentProductListResponseDto {
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

		/// <summary>
		/// 最新レート
		/// </summary>
		public double LatestRate {
			get;
			set;
		}

		/// <summary>
		/// 所有数
		/// </summary>
		public double Amount {
			get;
			set;
		}

		/// <summary>
		/// 平均取得価格
		/// </summary>
		public double AverageRate {
			get;
			set;
		}
	}

}