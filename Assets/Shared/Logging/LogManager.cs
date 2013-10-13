using UnityEngine;
using System.Collections;

public class LogManager {
	public static LogCategory General = null;
	public static LogCategory Online = null;
	public static LogCategory Chat = null;
	public static LogCategory DB = null;
	public static LogCategory Spam = null;
#if !LOBBY_SERVER
	// Reserved
#endif
	
	static LogManager() {
		string logPath = System.DateTime.UtcNow.ToString("yyyy-MM-dd/HH-mm-ss/");
		
		// Initialize log path
#if UNITY_STANDALONE_WIN
		LogCategory.Init("./Logs/" + logPath);
#else
		LogCategory.Init("./logs/" + logPath);
#endif
		
		// Create logs
		LogManager.General = new LogCategory("General");
		LogManager.Online = new LogCategory("Online");
		LogManager.Chat = new LogCategory("Chat");
		LogManager.DB = new LogCategory("DB");
		LogManager.Spam = new LogCategory("Spam", false);
#if !LOBBY_SERVER
		// Reserved
#endif
	}
}
