using System;

namespace Back.Models.HealthCheck.ResponseDto;

public class HealthCheckResultResponseDto {
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

	public int HealthCheckResultId {
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
}