using UnityEngine;
using System.Collections;

public class Version : SingletonMonoBehaviour<Version> {
	public string versionURL;
	
	// Start
	void Start() {
		WWW request = new WWW(versionURL);
		
		StartCoroutine(WaitForVersion(request));
	}
	
	// WaitForVersion
	IEnumerator WaitForVersion(WWW request) {
		yield return request;
		
		if(request.error == null) {
			versionNumber = int.Parse(request.text.Split('\n')[0].Split('=')[1]);
			LogManager.General.Log("Received game version number: " + versionNumber);
		} else {
			LogManager.General.LogError("Couldn't download game version information: " + request.error);
		}
	}
	
#region Properties
	// Version number
	public int versionNumber {
		get;
		protected set;
	}
#endregion
}
