using UnityEngine;
using uGameDB;
using Jboy;
using uLobby;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameDB {
	public static Dictionary<string, string> accountIdToName = new Dictionary<string, string>();
	public static Dictionary<string, Guild> guildIdToGuild = new Dictionary<string, Guild>();
	public static Dictionary<string, List<GuildMember>> guildIdToGuildMembers = new Dictionary<string, List<GuildMember>>();
	public static RankEntry[] rankingEntries;
	public static string logBucketPrefix = "<color=#ffcc00>";
	public static string logBucketMid = "</color>[<color=#00ffff>";
	public static string logBucketPostfix = "</color>]";
	public static int maxGuildNameLength = 50;
	public static int maxGuildTagLength = 4;
	
	public static void InitCodecs() {
		// Register JSON codec for player statistics
		Json.AddCodec<PlayerStats>(PlayerStats.JsonDeserializer, PlayerStats.JsonSerializer);
		Json.AddCodec<PlayerQueueStats>(PlayerQueueStats.JsonDeserializer, PlayerQueueStats.JsonSerializer);
		Json.AddCodec<CharacterStats>(CharacterStats.JsonDeserializer, CharacterStats.JsonSerializer);
		Json.AddCodec<InputControl>(InputControl.JsonDeserializer, InputControl.JsonSerializer);
		Json.AddCodec<InputSettings>(InputSettings.JsonDeserializer, InputSettings.JsonSerializer);
		Json.AddCodec<TimeStamp>(TimeStamp.JsonDeserializer, TimeStamp.JsonSerializer);
		Json.AddCodec<GuildMember>(GuildMember.JsonDeserializer, GuildMember.JsonSerializer);
		Json.AddCodec<Guild>(Guild.JsonDeserializer, Guild.JsonSerializer);
		Json.AddCodec<Texture2D>(GenericSerializer.Texture2DJsonDeserializer, GenericSerializer.Texture2DJsonSerializer);
		
		// Register JSON codecs for MapReduce entries
		Json.AddCodec<RankEntry>(RankEntry.JsonDeserializer, RankEntry.JsonSerializer);
		Json.AddCodec<AccountIdToNameEntry>(AccountIdToNameEntry.JsonDeserializer, AccountIdToNameEntry.JsonSerializer);
		
		// BitStream codecs
		uLink.BitStreamCodec.AddAndMakeArray<RankEntry>(RankEntry.ReadFromBitStream, RankEntry.WriteToBitStream);
		uLink.BitStreamCodec.AddAndMakeArray<ChatMember>(ChatMember.ReadFromBitStream, ChatMember.WriteToBitStream);
		uLink.BitStreamCodec.AddAndMakeArray<GuildMember>(GuildMember.ReadFromBitStream, GuildMember.WriteToBitStream);
	}
	
	// Delegate type
	public delegate void ActionOnResult<T>(T result);
	public delegate void PutActionOnResult<T>(string key, T result);
	
	// Resolve
	public static string Resolve(string key) {
		if(GameDB.accountIdToName.ContainsKey(key)) {
			return GameDB.accountIdToName[key] + " (" + key + ")";
		}
		
		if(GameDB.guildIdToGuild.ContainsKey(key)) {
			return GameDB.guildIdToGuild[key].name + " (" + key + ")";
		}
		
		return key;
	}
	
	// GetUniqueKey
	public static string GetUniqueKey() {
		return Hash(System.DateTime.UtcNow).ToString();
	}
	
	// Hash
	private static ulong Hash(System.DateTime when) {
	    ulong kind = (ulong) (int) when.Kind;
	    return (kind << 62) | (ulong) when.Ticks;
	}
	
	// Get
	public static IEnumerator Get<T>(string bucketName, string key, ActionOnResult<T> func) {
		var bucket = new Bucket(bucketName);
		var request = bucket.Get(key);
		yield return request.WaitUntilDone();
		
		string logInfo = logBucketPrefix + bucketName + logBucketMid + Resolve(key) + logBucketPostfix;
		
		if(request.isSuccessful) {
			T val = request.GetValue<T>();
			XDebug.Log("GET successful: " + logInfo + " -> " + val.ToString());
			func(val);
		} else {
			XDebug.LogWarning("GET failed: " + logInfo);
			func(default(T));
		}
	}
	
	// Set
	public static IEnumerator Set<T>(string bucketName, string key, T val, ActionOnResult<T> func) {
		var bucket = new Bucket(bucketName);
		var request = bucket.Set(key, val, Encoding.Json);
		yield return request.WaitUntilDone();
		
		string logInfo = logBucketPrefix + bucketName + logBucketMid + Resolve(key) + logBucketPostfix;
		
		if(request.isSuccessful) {
			XDebug.Log("SET successful: " + logInfo + " <- " + val.ToString());
			func(val);
		} else {
			XDebug.LogWarning("SET failed: " + logInfo + " <- " + val.ToString());
			func(default(T));
		}
	}
	
	// Put
	public static IEnumerator Put<T>(string bucketName, T val, PutActionOnResult<T> func) {
		var bucket = new Bucket(bucketName);
		var request = bucket.SetGeneratedKey(val, Encoding.Json);
		yield return request.WaitUntilDone();
		
		if(request.isSuccessful) {
			string generatedKey = request.GetGeneratedKey();
			string logInfo = logBucketPrefix + bucketName + logBucketMid + generatedKey + logBucketPostfix;
			
			XDebug.Log("PUT successful: " + logInfo + " <- " + val.ToString());
			func(generatedKey, val);
		} else {
			string logInfo = logBucketPrefix + bucketName + logBucketMid + logBucketPostfix;
			
			XDebug.LogWarning("PUT failed: " + logInfo + " <- " + val.ToString());
			func("", default(T));
		}
	}
	
	// MapReduce
	public static IEnumerator MapReduce<T>(string bucketName, string jsMapPhase, string jsReducePhase, object argument, ActionOnResult<T[]> func) {
		var bucket = new Bucket("AccountToName");
		var mapReduceRequest = bucket.MapReduce(
			new JavaScriptMapPhase(jsMapPhase),
			new JavaScriptReducePhase(jsReducePhase, argument)
		);
		
		// Wait until the request finishes
		yield return mapReduceRequest.WaitUntilDone();
		
		string logInfo = logBucketPrefix + bucketName + logBucketMid + argument.ToString() + logBucketPostfix;
		
		if(mapReduceRequest.isSuccessful) {
			var results = mapReduceRequest.GetResult<T>().ToArray();
			
			XDebug.Log("MapReduce successful: " + logInfo + " -> " + typeof(T).ToString() + "[" + results.Length + "]");
			func(results);
		} else {
			XDebug.LogWarning("MapReduce failed: " + logInfo + " -> " + mapReduceRequest.GetErrorString());
			func(default(T[]));
		}
	}
}
