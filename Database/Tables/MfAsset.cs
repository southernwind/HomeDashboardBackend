using System;

namespace Database.Tables; 
public class MfAsset {
	public DateOnly Date {
		get;
		set;
	}

	public string Institution {
		get;
		set;
	} = null!;

	public string Category {
		get;
		set;
	} = null!;

	public int Amount {
		get;
		set;
	}
	public bool IsLocked {
		get;
		set;
	}
}
