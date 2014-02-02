using UnityEngine;
using System.Collections;

public class Version : MonoBehaviour {
#if !LOBBY_SERVER
	public int versionNumber = 0;
#else
	[HideInInspector]
	public int versionNumber = 0;
	
	void Awake() {
		WWW request = new WWW("http://battleofmages.com/download/game.ini");
		
		StartCoroutine(WaitForVersion(request));
	}
	
	IEnumerator WaitForVersion(WWW request) {
		yield return request;
		
		if(request.error == null) {
			versionNumber = int.Parse(request.text.Split('\n')[0].Split('=')[1]);
			LogManager.General.Log("Received game version number: " + versionNumber);
		} else {
			LogManager.General.LogError("Couldn't download game version information: " + request.error);
		}
	}
#endif
}
