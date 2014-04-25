using UnityEngine;
using System.Collections;

public class WebStats : MonoBehaviour {
	public string webServerInfoURL;
	public int webServerInfoDelay;
	
	// Start
	void Start() {
		// Send stats to web server
#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
		InvokeRepeating("SendWebStats", 0.001f, webServerInfoDelay);
#endif
	}
	
	// Send web stats
	void SendWebStats() {
		var lobbyInfo = new string[] {
			"players=" + LobbyPlayer.list.Count,
			"townInstances=" + LobbyTown.running.Count,
			"ffaInstances=" + LobbyFFA.running.Count,
			"arenaInstances=" + LobbyMatch.running.Count,
			"cacheTime=" + (webServerInfoDelay + 1)
		};
		
		WWW request = new WWW(webServerInfoURL + "?" + string.Join("&", lobbyInfo));
		
		StartCoroutine(InformWebServer(request));
	}
	
	// Inform web server
	IEnumerator InformWebServer(WWW request) {
		yield return request;
		
		if(request.error == null) {
			LogManager.Spam.Log("Sent lobby information to web server");
		} else {
			LogManager.General.LogWarning("Couldn't reach web server: " + request.error);
		}
	}
}
