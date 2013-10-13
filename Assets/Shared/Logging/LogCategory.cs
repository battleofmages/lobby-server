#if !UNITY_WEBPLAYER
using System.IO;
#endif

#if UNITY_EDITOR || UNITY_WEBPLAYER
using UnityEngine;
#endif

public class LogCategory {
	public static string logPath = "./logs/"; 
	public static string timeFormat = "yyyy-MM-dd HH:mm:ss.fff";
	
#if !UNITY_WEBPLAYER
	public string filePath;
	private StreamWriter writer;
	private bool useUnityDebugLog;
#endif
	
	public static void Init(string newLogPath) {
		logPath = newLogPath;
		
#if !UNITY_WEBPLAYER
		if(!Directory.Exists(logPath))
			Directory.CreateDirectory(logPath);
#endif
	}
	
	public LogCategory(string categoryName, bool nUseUnityDebugLog = true) {
#if !UNITY_WEBPLAYER
		filePath = logPath + categoryName + ".log";
		writer = File.AppendText(filePath);
		writer.AutoFlush = true;
		useUnityDebugLog = nUseUnityDebugLog;
#endif
	}
	
	public void Log(object msg) {
#if !UNITY_WEBPLAYER
		writer.WriteLine(System.DateTime.UtcNow.ToString(timeFormat) + ": " + msg);
#endif
#if UNITY_EDITOR || UNITY_WEBPLAYER
		if(useUnityDebugLog)
			Debug.Log(msg);
#endif
	}
	
	public void LogWarning(object msg) {
#if !UNITY_WEBPLAYER
		writer.WriteLine(System.DateTime.UtcNow.ToString(timeFormat) + ": [WARNING] " + msg);
#endif
#if UNITY_EDITOR || UNITY_WEBPLAYER
		if(useUnityDebugLog)
			Debug.LogWarning(msg);
#endif
	}
	
	public void LogError(object msg) {
#if !UNITY_WEBPLAYER
		writer.WriteLine(System.DateTime.UtcNow.ToString(timeFormat) + ": [WARNING] " + msg);
#endif
#if UNITY_EDITOR || UNITY_WEBPLAYER
		if(useUnityDebugLog)
			Debug.LogError(msg);
#endif
	}
	
	public void Close() {
		writer.Close();
	}
}