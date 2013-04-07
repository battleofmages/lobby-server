using UnityEngine;
using uLobby;
using uGameDB;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LobbyGameDB : GameDB {
	public static Dictionary<string, string> accountIdToName = new Dictionary<string, string>();
	
	public static IEnumerator SetCharacterStats(LobbyPlayer lobbyPlayer, CharacterStats charStats) {
		Account account = lobbyPlayer.account;
		Debug.Log("Setting character stats for account '" + account.name + "' with ID '" + account.id.value + "' to " + charStats.ToString());
		
		var bucket = new Bucket("AccountToCharacterStats");
		var request = bucket.Set(account.id.value, charStats, Encoding.Json);
		yield return request.WaitUntilDone();
		
		if(request.isSuccessful) {
			lobbyPlayer.charStats = charStats;
			Debug.Log("Set character stats of '" + account.name + "' successfully: " + charStats.ToString());
		} else {
			Debug.LogWarning("Failed setting character stats of account '" + account.name + "' to " + charStats.ToString());
			Lobby.RPC("CharacterStatsSaveError", lobbyPlayer.peer);
		}
	}
	
	public static IEnumerator GetCharacterStats(LobbyPlayer lobbyPlayer) {
		Account account = lobbyPlayer.account;
		Debug.Log("Getting character stats for account '" + account.name + "' with ID '" + account.id.value + "'");
		
		var bucket = new Bucket("AccountToCharacterStats");
		var request = bucket.Get(account.id.value);
		yield return request.WaitUntilDone();
		
		if(request.isSuccessful) {
			lobbyPlayer.charStats = request.GetValue<CharacterStats>();
			Debug.Log("Retrieved character stats of '" + account.name + "' successfully: " + lobbyPlayer.charStats.ToString());
		} else {
			lobbyPlayer.charStats = new CharacterStats();
			Debug.Log("Account '" + account.name + "' doesn't have any character stats yet, creating default ones");
		}
		
		Lobby.RPC("ReceiveCharacterStats", lobbyPlayer.peer, lobbyPlayer.charStats);
	}
	
	// Get the player name
	public static IEnumerator GetPlayerName(LobbyPlayer lobbyPlayer) {
		Account account = lobbyPlayer.account;
		Debug.Log("Retrieving name for account " + account.name);
		
		var nameBucket = new Bucket("AccountToName");
		var nameRequest = nameBucket.Get(account.id.value);
		yield return nameRequest.WaitUntilDone();
		
		if(nameRequest.isSuccessful) {
			lobbyPlayer.name = nameRequest.GetValue<string>();
			
			Debug.Log("Queried player name of '" + account.name + "' successfully: " + lobbyPlayer.name);
			Lobby.RPC("ReceivePlayerInfo", lobbyPlayer.peer, account.id.value, lobbyPlayer.name);
			LobbyServer.OnReceivePlayerName(lobbyPlayer);
		} else {
			Debug.Log("Account " + account.name + " doesn't have a player name yet, asking him to enter one.");
			Lobby.RPC("AskPlayerName", lobbyPlayer.peer);
		}
	}
	
	// Sets the player name
	public static IEnumerator SetPlayerName(LobbyPlayer lobbyPlayer, string playerName) {
		Account account = lobbyPlayer.account;
		Debug.Log("Setting name for account '" + account.name + "' with ID '" + account.id.value + "' to '" + playerName + "'");
		
		var bucket = new Bucket("AccountToName");
		var request = bucket.Set(account.id.value, playerName, Encoding.Json);
		yield return request.WaitUntilDone();
		
		if(request.isSuccessful) {
			lobbyPlayer.name = playerName;
			Debug.Log("Set player name of '" + account.name + "' successfully: " + lobbyPlayer.name);
			
			Lobby.RPC("ReceivePlayerInfo", lobbyPlayer.peer, account.id.value, lobbyPlayer.name);
			LobbyServer.OnReceivePlayerName(lobbyPlayer);
		} else {
			Debug.LogWarning("Failed setting player name of account " + account.name + " to '" + playerName + "'.");
			Lobby.RPC("PlayerNameChangeError", lobbyPlayer.peer);
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
		
		// Assign stats
		lobbyPlayer.stats = statsInDB;
		
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
	
	// Sets last login date
	public static IEnumerator SetLastLoginDate(LobbyPlayer lobbyPlayer, System.DateTime timestamp) {
		string accountId = lobbyPlayer.account.id.value;
		
		var bucket = new Bucket("AccountToLastLoginDate");
		var request = bucket.Set(accountId, timestamp, Encoding.Json);
		yield return request.WaitUntilDone();
		
		if(request.isSuccessful) {
			Debug.Log("Set last login date of account ID '" + accountId + "' successfully: " + timestamp);
		} else {
			Debug.LogWarning("Failed setting last login date of account ID '" + accountId + "' to: " + timestamp);
		}
	}
	
	// Sets account registration date
	public static IEnumerator SetAccountRegistrationDate(string accountId, System.DateTime timestamp) {
		//Debug.Log("Setting account registration date for account ID '" + accountId + "' to '" + timestamp + "'");
		
		var bucket = new Bucket("AccountToRegistrationDate");
		var request = bucket.Set(accountId, timestamp, Encoding.Json);
		yield return request.WaitUntilDone();
		
		if(request.isSuccessful) {
			Debug.Log("Set registration date of account ID '" + accountId + "' successfully: " + timestamp);
		} else {
			Debug.LogWarning("Failed setting registration date of account ID '" + accountId + "' to: " + timestamp);
		}
	}
	
	// Gets account registration date
	public static IEnumerator GetAccountRegistrationDate(LobbyPlayer lobbyPlayer) {
		//Debug.Log("Getting account registration date for account ID '" + lobbyPlayer.account.id.value + "'");
		
		var bucket = new Bucket("AccountToRegistrationDate");
		var request = bucket.Get(lobbyPlayer.account.id.value);
		yield return request.WaitUntilDone();
		
		if(request.isSuccessful) {
			var timestamp = request.GetValue<System.DateTime>();
			Debug.Log("Got registration date of account ID '" + lobbyPlayer.account.id.value + "' successfully: " + timestamp);
		} else {
			Debug.LogWarning("Failed getting registration date of account ID '" + lobbyPlayer.account.id.value + "'");
		}
	}
	
	// Get top ranks
	public static IEnumerator GetTopRanks(uint maxPlayerCount, uLobby.LobbyPeer peer) {
		// Retrieve the highscore list from the database by using MapReduce. The MapReduce request consists of a
		// map phase and a reduce phase. The phases are expressed as JavaScript code in string form. The reduce
		// phase also gets the maximum number of scores to fetch as an argument.
		var bucket = new Bucket("AccountToStats");
		var getHighscoresRequest = bucket.MapReduce(new JavaScriptMapPhase(highscoresMapFunction),
													new JavaScriptReducePhase(highscoresReduceFunction, maxPlayerCount));
		
		// Wait until the request finishes and then update the local list of highscore entries.
		yield return getHighscoresRequest.WaitUntilDone();
		
		if(getHighscoresRequest.isSuccessful) {
			IEnumerable<RankEntry> rankingEntriesTmp = getHighscoresRequest.GetResult<RankEntry>();
			rankingEntries = rankingEntriesTmp.ToArray();
			
			// Get player names
			// TODO: Send X requests at once, then wait for all of them
			var nameBucket = new Bucket("AccountToName");
			var nameRequests = new GetRequest[rankingEntries.Length];
			for(int i = 0; i < rankingEntries.Length; i++) {
				var entry = rankingEntries[i];
				entry.rankIndex = i;
				
				if(LobbyGameDB.accountIdToName.ContainsKey(entry.accountId)) {
					entry.name = LobbyGameDB.accountIdToName[entry.accountId];
					nameRequests[i] = null;
				} else {
					nameRequests[i] = nameBucket.Get(entry.accountId);
				}
			}
			
			for(int i = 0; i < nameRequests.Length; i++) {
				var nameRequest = nameRequests[i];
				if(nameRequest == null)
					continue;
				
				yield return nameRequest.WaitUntilDone();
				
				if(nameRequest.isSuccessful) {
					var entry = rankingEntries[i];
					entry.name = nameRequest.GetValue<string>();
					LobbyGameDB.accountIdToName[entry.accountId] = entry.name;
				}
			}
			
			//Debug.Log("Sending the ranking list " + GameDB.rankingEntries + " with " + rankingEntries.Length + " entries");
			Lobby.RPC("ReceiveRankingList", peer, rankingEntries, false);
		} else {
			Debug.Log("Failed getting the ranking list: " + getHighscoresRequest.GetErrorString());
			Lobby.RPC("ReceiveRankingList", peer, null, false);
		}
	}
	
	// This is the JavaScript code for the map phase. The map phase operates on each key/value pair in the bucket
	// and should produce a list of any length. The list is then concatenated with the output of other map
	// operations and fed into the reduce phase. This map phase just parses the text value to a JSON object and
	// returns it as a one-element list.
	private const string highscoresMapFunction =
	@"
	function(value, keydata, arg) {
		var scoreEntry = JSON.parse(value.values[0].data);
		return [[0, value.key, '', scoreEntry.bestRanking]];
	}
	";
	
	// This is the JavaScript code for the reduce phase. The reduce phase operates on a combined list of the results
	// from any number of map phases, and should produce a new list. The resulting list can then be combined with
	// even more map phase results and fed into another reduce phase, so it is important that the reduce function
	// can be run many times on the same data without failing. This reduce phase sorts the items in the list
	// by score and trims away any item beyond the maxScoreCount argument that was sent along with the request.
	private const string highscoresReduceFunction =
	@"
	function(valueList, maxScoreCount) {
		var descendingOrder = function(a, b) { return b[3] - a[3]; };
		valueList.sort(descendingOrder);
		if (valueList.length > maxScoreCount) { valueList.length = maxScoreCount; }
		return valueList;
	}
	";
}
