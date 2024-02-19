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

	/// <summary>
	/// 預り区分リスト
	/// </summary>
	public TradingAccountCategory[] TradingAccountCategories {
		get;
		set;
	} = null!;

	public class TradingAccountCategory {
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
	}
}