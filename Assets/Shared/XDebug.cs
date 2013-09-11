using UnityEngine;
using System.Collections;

public class XDebug {
	public static string prefix = "<color=#808080>";
	public static string postfix = "</color>: ";
	public static string timeFormat = "MM/dd/yyyy hh:mm:ss.fff tt";
	
#if UNITY_EDITOR
	public static void Log(object msg) {
		Debug.Log(prefix + System.DateTime.Now.ToString(timeFormat) + postfix + msg);
	}
	
	public static void LogWarning(object msg) {
		Debug.LogWarning(prefix + System.DateTime.Now.ToString(timeFormat) + postfix + msg);
	}
	
	public static void LogError(object msg) {
		Debug.LogError(prefix + System.DateTime.Now.ToString(timeFormat) + postfix + msg);
	}
#else
	public static void Log(object msg) {
		Debug.Log(System.DateTime.Now.ToString(timeFormat) + ": " + msg);
	}
	
	public static void LogWarning(object msg) {
		Debug.LogWarning(System.DateTime.Now.ToString(timeFormat) + ": " + msg);
	}
	
	public static void LogError(object msg) {
		Debug.LogError(System.DateTime.Now.ToString(timeFormat) + ": " + msg);
	}
#endif
}
