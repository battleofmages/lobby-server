using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IPInfoServer : SingletonMonoBehaviour<IPInfoServer> {
	public static Dictionary<string, string> ipToCountry = new Dictionary<string, string>();
	public static Dictionary<string, string[]> ipToAccounts = new Dictionary<string, string[]>();
	public static Dictionary<string, string> accountIdToCountry = new Dictionary<string, string>();
	
	// Settings
	public string scriptURL = "http://battleofmages.com/scripts/ip2nation.php";
	public string scriptParamaterPrefix = "?ip=";
	
	// Start
	void Start () {
		// Make this class listen to lobby events
		//Lobby.AddListener(this);
	}
	
	// GetCountryByIP
	public static Coroutine GetCountryByIP(string ip, GameDB.ActionOnResult<string> func) {
		return IPInfoServer.instance.StartCoroutine(
			IPInfoServer.GetCountryByIPEnumerator(ip, func)
		);
	}
	
	// GetCountryByIPEnumerator
	public static IEnumerator GetCountryByIPEnumerator(string ip, GameDB.ActionOnResult<string> func) {
		var request = new WWW(IPInfoServer.instance.scriptURL + IPInfoServer.instance.scriptParamaterPrefix + ip);
		
		yield return request;
		
		if(request.error == null) {
			func(request.text);
		} else {
			func(default(string));
		}
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
}
