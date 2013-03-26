using UnityEngine;
using uLobby;
using uGameDB;
using System.Collections;
using System.Collections.Generic;

public class LobbyGameDB : GameDB {
	// Get the player name
	public static IEnumerator GetPlayerName(LobbyPlayer lobbyPlayer) {
		Account account = lobbyPlayer.account;
		Debug.Log("Retrieving name for account " + account.name);
		
		var bucket = new Bucket("AccountToName");
		var request = bucket.Get(account.id.value);
		yield return request.WaitUntilDone();
		
		if(request.isSuccessful) {
			lobbyPlayer.name = request.GetValue<string>();
			
			Debug.Log("Queried player name of '" + account.name + "' successfully: " + lobbyPlayer.name);
			Lobby.RPC("ReceivePlayerInfo", lobbyPlayer.peer, account.id.value, lobbyPlayer.name);
		} else {
			Debug.Log("Account " + account.name + " doesn't have a player name yet, asking him to enter one.");
			Lobby.RPC("AskPlayerName", lobbyPlayer.peer);
		}
	}
	
	// Sets the player name
	public static IEnumerator SetPlayerName(uLobby.Account account, string playerName) {
		Debug.Log("Setting name for account " + account.name + " with ID " + account.id.value + " to " + playerName);
		
		var bucket = new Bucket("AccountToName");
		var request = bucket.Set(account.id.value, playerName, Encoding.Json);
		yield return request.WaitUntilDone();
		
		if(request.isSuccessful) {
			Debug.Log("Set player name of '" + account.name + "' successfully: " + playerName);
			Lobby.RPC("ReceivePlayerInfo", AccountManager.Master.GetLoggedInPeer(account), account.id.value, playerName);
		} else {
			Debug.LogWarning("Failed setting player name of account " + account.name + " to '" + playerName + "'.");
			Lobby.RPC("PlayerNameChangeError", AccountManager.Master.GetLoggedInPeer(account));
		}
	}
	
	// Get stats for a single player
	public static IEnumerator GetPlayerStats(LobbyPlayer lobbyPlayer) {
		string accountId = lobbyPlayer.account.id.value;
		
		Debug.Log("Retrieving stats for account " + accountId);
		
		// Retrieve stats
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
		
		// Send the stats to the player
		Lobby.RPC("ReceivePlayerStats", lobbyPlayer.peer,
			Jboy.Json.WriteObject(statsInDB)
			/*statsInDB.level,
			statsInDB.bestRanking,
			statsInDB.ping,
			statsInDB.total*/
			/*statsInDB.queue[0],
			statsInDB.queue[1],
			statsInDB.queue[2],
			statsInDB.queue[3],
			statsInDB.queue[4]*/
		);
	}
}
