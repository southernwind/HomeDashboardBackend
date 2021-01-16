namespace Database.Tables {
	public class InvestmentProduct {
		/// <summary>
		/// 投資商品ID
		/// </summary>
		public int InvestmentProductId {
			get;
			set;
		}

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
	}
}
