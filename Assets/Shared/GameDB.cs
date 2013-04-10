using UnityEngine;
using uGameDB;
using System.Collections;
using System.Collections.Generic;
using Jboy;
using uLobby;

public class GameDB {
	public static Dictionary<string, string> accountIdToName = new Dictionary<string, string>();
	public static RankEntry[] rankingEntries;
	public static string logBucketPrefix = "<color=#ffcc00>";
	public static string logBucketMid = "</color>[<color=#00ffff>";
	public static string logBucketPostfix = "</color>]";
	
	public static void InitCodecs() {
		// Register JSON codec for player statistics
		Json.AddCodec<PlayerStats>(PlayerStats.JsonDeserializer, PlayerStats.JsonSerializer);
		Json.AddCodec<CharacterStats>(CharacterStats.JsonDeserializer, CharacterStats.JsonSerializer);
		Json.AddCodec<InputControl>(InputControl.JsonDeserializer, InputControl.JsonSerializer);
		Json.AddCodec<InputSettings>(InputSettings.JsonDeserializer, InputSettings.JsonSerializer);
		Json.AddCodec<TimeStamp>(TimeStamp.JsonDeserializer, TimeStamp.JsonSerializer);
		
		// Register JSON codec for rank entries
		uLink.BitStreamCodec.AddAndMakeArray<RankEntry>(RankEntry.ReadFromBitStream, RankEntry.WriteToBitStream);
		Json.AddCodec<RankEntry>(RankEntry.JsonDeserializer, RankEntry.JsonSerializer);
		
		// Chat member
		uLink.BitStreamCodec.AddAndMakeArray<ChatMember>(ChatMember.ReadFromBitStream, ChatMember.WriteToBitStream);
	}
	
	// Delegate type
	public delegate void ActionOnResult<T>(T result);
	
	// Resolve
	public static string Resolve(string key) {
		if(GameDB.accountIdToName.ContainsKey(key)) {
			return GameDB.accountIdToName[key];
		}
		
		return key;
	}
	
	// Get
	public static IEnumerator Get<T>(string bucketName, string key, ActionOnResult<T> func) {
		var bucket = new Bucket(bucketName);
		var request = bucket.Get(key);
		yield return request.WaitUntilDone();
		
		string logInfo = logBucketPrefix + bucketName + logBucketMid + Resolve(key) + logBucketPostfix;
		
		if(request.isSuccessful) {
			T val = request.GetValue<T>();
			Debug.Log("GET successful: " + logInfo + " -> " + val.ToString());
			func(val);
		} else {
			Debug.LogWarning("GET failed: " + logInfo);
			func(default (T));
		}
	}
	
	// Set
	public static IEnumerator Set<T>(string bucketName, string key, T val, ActionOnResult<T> func) {
		var bucket = new Bucket(bucketName);
		var request = bucket.Set(key, val, Encoding.Json);
		yield return request.WaitUntilDone();
		
		string logInfo = logBucketPrefix + bucketName + logBucketMid + Resolve(key) + logBucketPostfix;
		
		if(request.isSuccessful) {
			Debug.Log("SET successful: " + logInfo + " <- " + val.ToString());
			func(val);
		} else {
			Debug.LogWarning("SET failed: " + logInfo + " <- " + val.ToString());
			func(default (T));
		}
	}
}
