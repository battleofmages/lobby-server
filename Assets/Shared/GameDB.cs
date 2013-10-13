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
	public static List<List<RankEntry[]>> rankingLists;
	public static string logBucketPrefix = ""; //"<color=#ffcc00>";
	public static string logBucketMid = "["; //"</color>[<color=#00ffff>";
	public static string logBucketPostfix = "]"; //"</color>]";
	public static int maxPlayerNameLength = 25;
	public static int maxGuildNameLength = 30;
	public static int maxGuildTagLength = 4;
	public static int numRankingPages = 7;
	
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
		Json.AddCodec<GuildList>(GuildList.JsonDeserializer, GuildList.JsonSerializer);
		Json.AddCodec<CharacterCustomization>(CharacterCustomization.JsonDeserializer, CharacterCustomization.JsonSerializer);
		Json.AddCodec<PaymentsList>(PaymentsList.JsonDeserializer, PaymentsList.JsonSerializer);
		
		// Register JSON codecs for integrated types
		Json.AddCodec<Color>(GenericSerializer.ColorJsonDeserializer, GenericSerializer.ColorJsonSerializer);
		Json.AddCodec<Texture2D>(GenericSerializer.Texture2DJsonDeserializer, GenericSerializer.Texture2DJsonSerializer);
		
		// Register JSON codecs for Artifacts
		Json.AddCodec<Artifact>(Artifact.JsonDeserializer, Artifact.JsonSerializer);
		Json.AddCodec<ArtifactSlot>(ArtifactSlot.JsonDeserializer, ArtifactSlot.JsonSerializer);
		Json.AddCodec<ArtifactTree>(ArtifactTree.JsonDeserializer, ArtifactTree.JsonSerializer);
		Json.AddCodec<ArtifactInventory>(ArtifactInventory.JsonDeserializer, ArtifactInventory.JsonSerializer);
		
		// Register JSON codecs for Inventory
		Json.AddCodec<Inventory>(Inventory.JsonDeserializer, Inventory.JsonSerializer);
		Json.AddCodec<ItemSlot>(ItemSlot.JsonDeserializer, ItemSlot.JsonSerializer);
		
		// Register JSON codecs for SkillBuilds
		Json.AddCodec<SkillBuild>(SkillBuild.JsonDeserializer, SkillBuild.JsonSerializer);
		Json.AddCodec<WeaponBuild>(WeaponBuild.JsonDeserializer, WeaponBuild.JsonSerializer);
		Json.AddCodec<AttunementBuild>(AttunementBuild.JsonDeserializer, AttunementBuild.JsonSerializer);
		
		// Register JSON codecs for MapReduce entries
		Json.AddCodec<RankEntry>(RankEntry.JsonDeserializer, RankEntry.JsonSerializer);
		Json.AddCodec<KeyToValueEntry>(KeyToValueEntry.JsonDeserializer, KeyToValueEntry.JsonSerializer);
		
		// BitStream codecs
		uLink.BitStreamCodec.AddAndMakeArray<RankEntry>(RankEntry.ReadFromBitStream, RankEntry.WriteToBitStream);
		uLink.BitStreamCodec.AddAndMakeArray<ChatMember>(ChatMember.ReadFromBitStream, ChatMember.WriteToBitStream);
		uLink.BitStreamCodec.AddAndMakeArray<GuildMember>(GuildMember.ReadFromBitStream, GuildMember.WriteToBitStream);
		uLink.BitStreamCodec.AddAndMakeArray<SkillBuild>(SkillBuild.ReadFromBitStream, SkillBuild.WriteToBitStream);
		uLink.BitStreamCodec.AddAndMakeArray<WeaponBuild>(WeaponBuild.ReadFromBitStream, WeaponBuild.WriteToBitStream);
		uLink.BitStreamCodec.AddAndMakeArray<AttunementBuild>(AttunementBuild.ReadFromBitStream, AttunementBuild.WriteToBitStream);
		uLink.BitStreamCodec.AddAndMakeArray<CharacterCustomization>(CharacterCustomization.ReadFromBitStream, CharacterCustomization.WriteToBitStream);
	}
	
	public static void InitRankingLists() {
		// Subject -> Queue -> RankEntry
		rankingLists = new List<List<RankEntry[]>>();
		
		// Player
		rankingLists.Add(new List<RankEntry[]>());
		
		// Team
		rankingLists.Add(new List<RankEntry[]>());
		
		// Guild
		rankingLists.Add(new List<RankEntry[]>());
		
		// Fill with null
		foreach(var list in rankingLists) {
			for(byte i = 0; i < 7; i++) {
				list.Add(null);
			}
		}
	}
	
	// Delegate type
	public delegate void ActionOnResult<T>(T result);
	public delegate void PutActionOnResult<T>(string key, T result);
	
	// Resolve
	public static string Resolve(string key) {
		if(GameDB.accountIdToName.ContainsKey(key)) {
			return GameDB.accountIdToName[key]; //+ " (" + key + ")";
		}
		
		if(GameDB.guildIdToGuild.ContainsKey(key)) {
			return GameDB.guildIdToGuild[key].name; //+ " (" + key + ")";
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
	
	// Encrypt a password using SHA1
	public static byte[] EncryptPassword(string password) {
		// Make precalculated, generic rainbow tables ineffective by using a salt
		string salt = "c90e8eca04f64d70baacc9d0a5c4c72e" + password;
		password += salt;
		
		// Encrypt the password
		System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1.Create();
		return sha1.ComputeHash(System.Text.Encoding.Unicode.GetBytes(password));
	}
	
	static string FormatBucketName(string bucketName) {
		var toIndex = bucketName.IndexOf("To");
		if(toIndex != -1) {
			return bucketName.Substring(toIndex + 2);
		}
		
		return bucketName;
	}
	
	static string FormatSuccess(string key, string operation, string bucketName, object val) {
		if(operation != "get")
			return Resolve(key) + "." + operation + FormatBucketName(bucketName) + "(" + val.ToString() + ")";
		
		return Resolve(key) + "." + operation + FormatBucketName(bucketName) + "() -> " + val.ToString();
	}
	
	static string FormatFail(string key, string operation, string bucketName) {
		return Resolve(key) + "." + operation + FormatBucketName(bucketName) + "() FAIL";
	}
	
	// Get
	public static IEnumerator Get<T>(string bucketName, string key, ActionOnResult<T> func) {
		var bucket = new Bucket(bucketName);
		var request = bucket.Get(key);
		yield return request.WaitUntilDone();
		
		if(request.isSuccessful) {
			T val = request.GetValue<T>();
			LogManager.DB.Log(FormatSuccess(key, "get", bucketName, val));
			func(val);
		} else {
			LogManager.DB.LogWarning(FormatFail(key, "get", bucketName));
			func(default(T));
		}
	}
	
	// Set
	public static IEnumerator Set<T>(string bucketName, string key, T val, ActionOnResult<T> func) {
		var bucket = new Bucket(bucketName);
		var request = bucket.Set(key, val, Encoding.Json);
		yield return request.WaitUntilDone();
		
		if(request.isSuccessful) {
			LogManager.DB.Log(FormatSuccess(key, "set", bucketName, val));
			if(func != null)
				func(val);
		} else {
			LogManager.DB.LogWarning(FormatFail(key, "set", bucketName));
			if(func != null)
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
			
			LogManager.DB.Log(FormatSuccess(generatedKey, "put", bucketName, val));
			func(generatedKey, val);
		} else {
			LogManager.DB.LogWarning(FormatFail("", "put", bucketName));
			func(default(string), default(T));
		}
	}
	
	// Remove
	public static IEnumerator Remove(string bucketName, string key, ActionOnResult<bool> func) {
		var bucket = new Bucket(bucketName);
		var request = bucket.Remove(key);
		yield return request.WaitUntilDone();
		
		if(request.isSuccessful) {
			func(true);
		} else {
			LogManager.DB.LogWarning(FormatFail(key, "remove", bucketName));
			func(false);
		}
	}
	
	// MapReduce
	public static IEnumerator MapReduce<T>(string bucketName, string jsMapPhase, string jsReducePhase, object argument, ActionOnResult<T[]> func) {
		var bucket = new Bucket(bucketName);
		var mapReduceRequest = bucket.MapReduce(
			new JavaScriptMapPhase(jsMapPhase),
			new JavaScriptReducePhase(jsReducePhase, argument)
		);
		
		// Wait until the request finishes
		yield return mapReduceRequest.WaitUntilDone();
		
		string logInfo = logBucketPrefix + bucketName + logBucketMid + argument.ToString() + logBucketPostfix;
		
		if(mapReduceRequest.isSuccessful) {
			var results = mapReduceRequest.GetResult<T>().ToArray();
			
			LogManager.DB.Log("MapReduce successful: " + logInfo + " -> " + typeof(T).ToString() + "[" + results.Length + "]");
			func(results);
		} else {
			LogManager.DB.LogWarning("MapReduce failed: " + logInfo + " -> " + mapReduceRequest.GetErrorString());
			func(default(T[]));
		}
	}
	
	// --------------------------------------------------------------------------------
	// Generic MapReduce
	// --------------------------------------------------------------------------------
	
	// Map
	public static string GetSearchMapFunction(string property) {
		return @"
			function(value, keydata, arg) {
				var nameEntry = JSON.parse(value.values[0].data);
				return [[value.key, nameEntry." + property + @"]];
			}
		";
	}
	
	// Reduce
	public static string GetSearchReduceFunction() {
		return @"
			function(valueList, nameToFind) {
				var length = valueList.length;
				var element = null;
				
				for(var i = 0; i < length; i++) {
					element = valueList[i];
					if(element[1] == nameToFind) {
						return [element];
					}
				}
				
				return [];
			}
		";
	}
}
