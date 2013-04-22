using UnityEngine;
using uLobby;
using uGameDB;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LobbyGameDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// Player
	// --------------------------------------------------------------------------------
	
	// Get the player name
	public IEnumerator GetPlayerName(string accountId, GameDB.ActionOnResult<string> func) {
		yield return StartCoroutine(GameDB.Get<string>(
			"AccountToName",
			accountId,
			func
		));
	}
	
	// Sets the player name
	public IEnumerator SetPlayerName(LobbyPlayer lobbyPlayer, string playerName) {
		yield return StartCoroutine(GameDB.Set<string>(
			"AccountToName",
			lobbyPlayer.accountId,
			playerName,
			data => {
				if(data == null) {
					Lobby.RPC("PlayerNameChangeError", lobbyPlayer.peer);
				} else {
					lobbyPlayer.name = data;
					Lobby.RPC("ReceivePlayerInfo", lobbyPlayer.peer, lobbyPlayer.accountId, lobbyPlayer.name);
					LobbyServer.OnReceivePlayerName(lobbyPlayer);
				}
			}
		));
	}
	
	// Get stats for a single player
	public IEnumerator GetPlayerStats(LobbyPlayer lobbyPlayer) {
		yield return StartCoroutine(GameDB.Get<PlayerStats>(
			"AccountToStats",
			lobbyPlayer.accountId,
			data => {
				if(data == null)
					data = new PlayerStats();
				
				// Assign stats
				lobbyPlayer.stats = data;
				
				// Send the stats to the player
				Lobby.RPC("ReceivePlayerStats", lobbyPlayer.peer,
					Jboy.Json.WriteObject(data)
				);
			}
		));
	}
	
	// Sets last login date
	public IEnumerator SetLastLoginDate(LobbyPlayer lobbyPlayer, System.DateTime timestamp) {
		yield return StartCoroutine(GameDB.Set<TimeStamp>(
			"AccountToLastLoginDate",
			lobbyPlayer.accountId,
			new TimeStamp(timestamp),
			data => {
				// ...
			}
		));
	}
	
	// Sets account registration date
	public IEnumerator SetAccountRegistrationDate(string accountId, System.DateTime timestamp) {
		yield return StartCoroutine(GameDB.Set<TimeStamp>(
			"AccountToRegistrationDate",
			accountId,
			new TimeStamp(timestamp),
			data => {
				// ...
			}
		));
	}
	
	// Gets account registration date
	public IEnumerator GetAccountRegistrationDate(LobbyPlayer lobbyPlayer) {
		/*yield return StartCoroutine(GameDB.Get<TimeStamp>(
		"AccountToRegistrationDate",
		lobbyPlayer.accountId,
		data => {
			if(data == null)
				XDebug.LogWarning("Failed getting registration date of account ID '" + lobbyPlayer.accountId + "'");
			else
				XDebug.Log("Got registration date of account ID '" + lobbyPlayer.accountId + "' successfully: " + data);
		}));*/
		yield break;
	}
	
	// --------------------------------------------------------------------------------
	// AccountToEmail
	// --------------------------------------------------------------------------------
	
	// Set email
	public IEnumerator SetEmail(string accountId, string email, GameDB.ActionOnResult<string> func) {
		yield return StartCoroutine(GameDB.Set<string>(
			"AccountToEmail",
			accountId,
			email,
			func
		));
	}
	
	// Get email
	public IEnumerator GetEmail(string accountId, GameDB.ActionOnResult<string> func) {
		yield return StartCoroutine(GameDB.Get<string>(
			"AccountToEmail",
			accountId,
			func
		));
	}
	
	// --------------------------------------------------------------------------------
	// MapReduce
	// --------------------------------------------------------------------------------
	
	// Get account ID by player name
	public IEnumerator GetAccountIdByPlayerName(string playerName, GameDB.ActionOnResult<string> func) {
		yield return StartCoroutine(GameDB.MapReduce<KeyToValueEntry>(
			"AccountToName",
			GameDB.GetSearchMapFunction("v"),
			GameDB.GetSearchReduceFunction(),
			playerName,
			data => {
				if(data != null && data.Length == 1) {
					func(data[0].key);
				} else {
					func(default(string));
				}
			}
		));
	}
	
	// Get account ID by Email
	public IEnumerator GetAccountIdByEmail(string email, GameDB.ActionOnResult<string> func) {
		yield return StartCoroutine(GameDB.MapReduce<KeyToValueEntry>(
			"AccountToEmail",
			GameDB.GetSearchMapFunction("v"),
			GameDB.GetSearchReduceFunction(),
			email,
			data => {
				if(data != null && data.Length == 1) {
					func(data[0].key);
				} else {
					func(default(string));
				}
			}
		));
	}
}
