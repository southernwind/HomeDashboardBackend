using System;

namespace Database.Tables {
	public class HealthCheckResult {
		private HealthCheckTarget? _healthCheckTarget;

		public int HealthCheckResultId {
			get;
			set;
		}

		public int HealthCheckTargetId {
			get;
			set;
		}

		public DateTime DateTime {
			get;
			set;
		}

		public bool State {
			get;
			set;
		}

		public string? Reason {
			get;
			set;
		}

		public virtual HealthCheckTarget HealthCheckTarget {
			get {
				return this._healthCheckTarget ?? throw new InvalidOperationException();
			}
			set {
				this._healthCheckTarget = value;
			}
		}
	}
}
