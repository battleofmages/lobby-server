using UnityEngine;
using uGameDB;
using System.Collections;
using System.Collections.Generic;
using Jboy;
using uLobby;

[System.Serializable]
public class RankEntry {
	public int rankIndex;
	public string accountId;
	public string name;
	public int bestRanking;
	
	public static void WriteRankEntry(uLink.BitStream stream, object val, params object[] args) {
		//Debug.Log("WriteRankEntry");
		RankEntry myObj = (RankEntry)val;
		stream.WriteInt32(myObj.rankIndex);
		stream.WriteString(myObj.accountId);
		stream.WriteString(myObj.name);
		stream.WriteInt32(myObj.bestRanking);
		//Debug.Log("WriteRankEntry: " + myObj.accountId + ", " + myObj.bestRanking);
	}
	
	public static object ReadRankEntry(uLink.BitStream stream, params object[] args) {
		//Debug.Log("ReadRankEntry");
		RankEntry myObj = new RankEntry();
		myObj.rankIndex = stream.ReadInt32();
		myObj.accountId = stream.ReadString();
		myObj.name = stream.ReadString();
		myObj.bestRanking = stream.ReadInt32();
		//Debug.Log("ReadRankEntry: " + myObj.accountId + ", " + myObj.bestRanking);
		return myObj;
	}
	
	// Writer
	public static void JsonSerializer(JsonWriter writer, object instance) {
		var scoreEntry = (RankEntry)instance;
		
		writer.WriteArrayStart();
		writer.WriteNumber(scoreEntry.rankIndex);
		writer.WriteString(scoreEntry.accountId);
		writer.WriteString(scoreEntry.name);
		writer.WriteNumber(scoreEntry.bestRanking);
		writer.WriteArrayEnd();
	}
	
	// Reader
	public static object JsonDeserializer(JsonReader reader) {
		var scoreEntry = new RankEntry();
		
		reader.ReadArrayStart();
		scoreEntry.rankIndex = (int)reader.ReadNumber();
		scoreEntry.accountId = reader.ReadString();
		scoreEntry.name = reader.ReadString();
		scoreEntry.bestRanking = (int)reader.ReadNumber();
		reader.ReadArrayEnd();
		
		return scoreEntry;
	}
}

public class GameDB {
	public static RankEntry[] rankingEntries;
	
	public static void InitCodecs() {
		// Register JSON codec for player statistics
		Json.AddCodec<PlayerStats>(PlayerStats.JsonDeserializer, PlayerStats.JsonSerializer);
		Json.AddCodec<CharacterStats>(CharacterStats.JsonDeserializer, CharacterStats.JsonSerializer);
		
		// Register JSON codec for rank entries
		uLink.BitStreamCodec.AddAndMakeArray<RankEntry>(RankEntry.ReadRankEntry, RankEntry.WriteRankEntry);
		Json.AddCodec<RankEntry>(RankEntry.JsonDeserializer, RankEntry.JsonSerializer);
	}
	
	// Send stats for a single account
	public static IEnumerator SendAccountStats(string accountId, PlayerStats stats) {
		//Debug.Log("Going to send player stats of '" + player.GetName() + "' to the database");
		Debug.Log("Retrieving stats for account " + accountId);
		
		// Retrieve old stats
		var bucket = new Bucket("AccountToStats");
		var getRequest = bucket.Get(accountId);
		yield return getRequest.WaitUntilDone();
		
		PlayerStats statsInDB;
		
		if(getRequest.isSuccessful) {
			statsInDB = getRequest.GetValue<PlayerStats>();
			
			Debug.Log("Queried stats of account '" + accountId + "' successfully (Ranking: " + statsInDB.ranking + ")");
		} else {
			statsInDB = new PlayerStats();
			
			Debug.Log("Account " + accountId + " doesn't have any player stats yet");
		}
		
		// Reset
		//PlayerStats statsInDB = new PlayerStats();
		
		// Calculate new stats
		statsInDB.MergeWithMatch(stats);
		
		// Write new stats
		var setRequest = bucket.Set(accountId, statsInDB, Encoding.Json);
		yield return setRequest.WaitUntilDone();
		
		if(setRequest.isSuccessful) {
			Debug.Log("Wrote account stats of '" + accountId + "' successfully (Ranking: " + statsInDB.ranking + ", Level: " + statsInDB.level + ")");
		} else {
			Debug.LogWarning("Could not write account stats for '" + accountId + "'");
		}
	}
}
