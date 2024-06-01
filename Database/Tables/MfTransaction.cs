using System;

namespace Database.Tables {
	public class MfTransaction {
		/// <summary>
		/// Transaction ID
		/// </summary>
		public string TransactionId {
			get;
			set;
		} = null!;

		/// <summary>
		/// 計算対象
		/// </summary>
		public bool IsCalculateTarget {
			get;
			set;
		}

		/// <summary>
		/// 日付
		/// </summary>
		public DateTime Date {
			get;
			set;
		}

		/// <summary>
		/// 内容
		/// </summary>
		public string Content {
			get;
			set;
		} = null!;

		/// <summary>
		/// 金額(円)
		/// </summary>
		public int Amount {
			get;
			set;
		}

		/// <summary>
		/// 金融機関
		/// </summary>
		public string Institution {
			get;
			set;
		} = null!;

		/// <summary>
		/// 大項目
		/// </summary>
		public string LargeCategory {
			get;
			set;
		} = null!;

		/// <summary>
		/// 中項目
		/// </summary>
		public string MiddleCategory {
			get;
			set;
		} = null!;

		/// <summary>
		/// メモ
		/// </summary>
		public string Memo {
			get;
			set;
		} = null!;
		public bool IsLocked {
			get;
			set;
		}
	}
}
