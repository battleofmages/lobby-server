using UnityEngine;
using System.Collections;

public class XDebug {
	public static string prefix = "<color=#808080>";
	public static string postfix = "</color>: ";
	
	public static void Log(object msg) {
		Debug.Log(prefix + System.DateTime.Now + postfix + msg);
	}
	
	public static void LogWarning(object msg) {
		Debug.LogWarning(prefix + System.DateTime.Now + postfix + msg);
	}
	
	public static void LogError(object msg) {
		Debug.LogError(prefix + System.DateTime.Now + postfix + msg);
	}
}
