using System;

namespace Back.Models.Financial.ResponseDto {
	public class GetTradingAccountDetailDto {
		/// <summary>
		/// 証券口座名
		/// </summary>
		public string TradingAccountName {
			get;
			set;
		} = null!;

		/// <summary>
		/// 証券口座ロゴ
		/// </summary>
		public string TradingAccountLogo {
			get;
			set;
		} = null!;

		/// <summary>
		/// 取得量サマリーリスト
		/// </summary>
		public TradingAccountDetailAmountSummary[] TradingAccountDetailAmountSummaryList {
			get;
			set;
		} = null!;

		/// <summary>
		/// 取得量リスト
		/// </summary>
		public TradingAccountDetailAmount[] TradingAccountDetailAmountList {
			get;
			set;
		} = null!;

		public class TradingAccountDetailAmountSummary {
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
			/// カテゴリ
			/// </summary>
			public string Category {
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
			/// 通貨ID
			/// </summary>
			public int CurrencyUnitId {
				get;
				set;
			}

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

			/// <summary>
			/// 預り区分別取得量リスト
			/// </summary>
			public TradingAccountCategoryDetailAmount[] TradingAccountCategoryDetailAmountList {
				get;
				set;
			} = null!;

			public class TradingAccountCategoryDetailAmount {
				/// <summary>
				/// 預り区分名
				/// </summary>
				public string TradingAccountCategoryName {
					get;
					set;
				} = null!;

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

		public class TradingAccountDetailAmount {
			/// <summary>
			/// 預り区分名
			/// </summary>
			public string TradingAccountCategoryName {
				get;
				set;
			} = null!;

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
			public string InvestmentProductName {
				get;
				set;
			} = null!;

			/// <summary>
			/// 投資商品取得量ID
			/// </summary>
			public int InvestmentProductAmountId {
				get;
				set;
			}

			/// <summary>
			/// 通貨単位
			/// </summary>
			public int CurrencyUnitId {
				get;
				set;
			}

			/// <summary>
			/// 取得日
			/// </summary>
			public DateTime Date {
				get;
				set;
			}

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
	}
}