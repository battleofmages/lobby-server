using UnityEngine;
using uGameDB;
using System.Collections;
using System.Collections.Generic;

public class GameDB {
	public static void InitCodecs() {
		Jboy.Json.AddCodec<PlayerStats>(PlayerStats.JsonDeserializer, PlayerStats.JsonSerializer);
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
