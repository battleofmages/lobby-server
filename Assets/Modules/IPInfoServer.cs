using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class IPInfoServer : MonoBehaviour {
	public static Dictionary<string, string> ipToCountry = new Dictionary<string, string>();
	public static Dictionary<string, string[]> ipToAccounts = new Dictionary<string, string[]>();
	public static Dictionary<string, string> accountIdToCountry = new Dictionary<string, string>();
	public static string ip2nationURL = "http://battle-of-mages.com/scripts/ip2nation.php?ip=";
	//private IPInfoDB ipInfoDB;
	
	void Start () {
		//ipInfoDB = this.GetComponent<IPInfoDB>();
		
		// Make this class listen to lobby events
		//Lobby.AddListener(this);
	}
	
	public static IEnumerator GetCountryByIP(string ip) {
		var request = new WWW(ip2nationURL + ip);
		
		yield return request;
		
		if(request.error == null) {
			string country = request.text;
			ipToCountry[ip] = country;
			LogManager.Online.Log("IP '" + ip + "' comes from '" + country + "'");
		}
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
}
