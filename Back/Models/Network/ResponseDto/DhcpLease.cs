using Database.Tables;

namespace Back.Models.Network.ResponseDto; 
public class DhcpLease(string timeOfLeaseExpiry, string macAddress, string ipAddress, string hostName, string clientId) {
	public string TimeOfLeaseExpiry {
		get;
		set;
	} = timeOfLeaseExpiry;

	public string MacAddress {
		get;
		set;
	} = macAddress;

	public string IpAddress {
		get;
		set;
	} = ipAddress;

	public string HostName {
		get;
		set;
	} = hostName;

	public string ClientId {
		get;
		set;
	} = clientId;

	public MacAddressVendor? Vendor {
		get;
		set;
	}
}
