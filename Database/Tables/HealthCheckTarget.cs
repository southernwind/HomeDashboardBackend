using System.Collections.Generic;
using System;

namespace Database.Tables; 
public class HealthCheckTarget {
	private ICollection<HealthCheckResult>? _healthCheckResults;

	public int HealthCheckTargetId {
		get;
		set;
	}

	public string Name {
		get;
		set;
	} = null!;

	public string Host {
		get;
		set;
	} = null!;

	public bool IsEnable {
		get;
		set;
	}

	public int CheckType {
		get;
		set;
	}

	public virtual ICollection<HealthCheckResult> HealthCheckResults{
		get {
			return this._healthCheckResults ?? throw new InvalidOperationException();
		}
		set {
			this._healthCheckResults = value;
		}
	}
}
