using uLobby;
using UnityEngine;
using System.Collections;

public static class LobbyGameDB {
	// --------------------------------------------------------------------------------
	// Player
	// --------------------------------------------------------------------------------
	
	// Get the player name
	public static Coroutine GetPlayerName(string accountId, GameDB.ActionOnResult<string> func) {
		return GameDB.instance.StartCoroutine(GameDB.Get<string>(
			"AccountToName",
			accountId,
			func
		));
	}

	// Sets the player name
	public static Coroutine SetPlayerName(string accountId, string playerName, GameDB.ActionOnResult<string> func = null) {
		return GameDB.instance.StartCoroutine(GameDB.Set<string>(
			"AccountToName",
			accountId,
			playerName,
			func
		));
	}

	/*
	
	// Sets the player name
	public static Coroutine SetPlayerName(LobbyPlayer player, string playerName) {
		return GameDB.instance.StartCoroutine(GameDB.Set<string>(
			"AccountToName",
			player.accountId,
			playerName,
			data => {
				if(data == null) {
					Lobby.RPC("PlayerNameChangeError", player.peer);
				} else {
					player.name = data;
					Lobby.RPC("ReceivePlayerName", player.peer, player.accountId, player.name);
					LobbyServer.OnReceivePlayerName(player);
				}
			}
		));
	}
	
	// Get stats for a single player
	public static Coroutine GetPlayerStats(string accountId, GameDB.ActionOnResult<PlayerStats> func) {
		return GameDB.instance.StartCoroutine(GameDB.Get<PlayerStats>(
			"AccountToStats",
			accountId,
			func
		));
	}
	
	// Get stats for a single player
	public static Coroutine GetPlayerStats(LobbyPlayer player) {
		return GameDB.instance.StartCoroutine(GameDB.Get<PlayerStats>(
			"AccountToStats",
			player.accountId,
			data => {
				if(data == null)
					data = new PlayerStats();
				
				// Assign stats
				player.stats = data;
				
				// Send the stats to the player
				Lobby.RPC("ReceivePlayerStats", player.peer,
					player.accountId,
					Jboy.Json.WriteObject(data)
				);
			}
		));
	}
	*/
	// Sets last login date
	public static Coroutine SetLastLoginDate(string accountId, System.DateTime timestamp) {
		return GameDB.instance.StartCoroutine(GameDB.Set<TimeStamp>(
			"AccountToLastLoginDate",
			accountId,
			new TimeStamp(timestamp),
			data => {
				// ...
			}
		));
	}
	
	// Gets account registration date
	public static Coroutine GetAccountRegistrationDate(string accountId) {
		return GameDB.instance.StartCoroutine(GameDB.Get<TimeStamp>(
			"AccountToRegistrationDate",
			accountId,
			data => {
				if(data == null)
					LogManager.General.LogWarning("Failed getting registration date of account ID '" + accountId + "'");
				else
					LogManager.General.Log("Got registration date of account ID '" + accountId + "' successfully: " + data);
			}
		));
	}

	// Sets account registration date
	public static Coroutine SetAccountRegistrationDate(string accountId, System.DateTime timestamp) {
		return GameDB.instance.StartCoroutine(GameDB.Set<TimeStamp>(
			"AccountToRegistrationDate",
			accountId,
			new TimeStamp(timestamp),
			data => {
				// ...
			}
		));
	}
	
	// --------------------------------------------------------------------------------
	// AccountToEmail
	// --------------------------------------------------------------------------------

	// Get email
	public static Coroutine GetEmail(string accountId, GameDB.ActionOnResult<string> func) {
		return GameDB.instance.StartCoroutine(GameDB.Get<string>(
			"AccountToEmail",
			accountId,
			func
		));
	}

	// Set email
	public static Coroutine SetEmail(string accountId, string email, GameDB.ActionOnResult<string> func) {
		return GameDB.instance.StartCoroutine(GameDB.Set<string>(
			"AccountToEmail",
			accountId,
			email,
			func
		));
	}

	/*
	// --------------------------------------------------------------------------------
	// Password
	// --------------------------------------------------------------------------------
	
	// Set password hash
	public static IEnumerator SetPassword(LobbyPlayer player, string newPassword) {
		var req = AccountManager.Master.UpdateAccount(player.account, new AccountUpdate() {
			password = newPassword
		});
		
		req.OnSuccessful += (request) => {
			Lobby.RPC("PasswordChangeSuccess", player.peer);
		};
		
		req.OnException += (request, exception) => {
			Lobby.RPC("PasswordChangeError", player.peer, exception.ToString());
		};
		
		yield return req;
	}
	*/
	
	// --------------------------------------------------------------------------------
	// AccountsAwaitingActivation
	// --------------------------------------------------------------------------------
	
	// Adds an account to the activation waiting list
	public static Coroutine PutAccountAwaitingActivation(string email, GameDB.ActionOnResult<string> func) {
		var token = GameDB.GetUniqueKey();
		
		return GameDB.instance.StartCoroutine(GameDB.Set<string>(
			"AccountsAwaitingActivation",
			email,
			token,
			func
		));
	}
	
	// Is the account in the activation wait list?
	public static Coroutine GetAccountAwaitingActivation(string email, GameDB.ActionOnResult<string> func) {
		return GameDB.instance.StartCoroutine(GameDB.Get<string>(
			"AccountsAwaitingActivation",
			email,
			func
		));
	}


	// --------------------------------------------------------------------------------
	// MapReduce
	// --------------------------------------------------------------------------------
	
	// Get account ID by player name
	public static Coroutine GetAccountIdByPlayerName(string playerName, GameDB.ActionOnResult<string> func) {
		return GameDB.instance.StartCoroutine(GameDB.MapReduce<KeyValue<string>>(
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
	public static Coroutine GetAccountIdByEmail(string email, GameDB.ActionOnResult<string> func) {
		return GameDB.instance.StartCoroutine(GameDB.MapReduce<KeyValue<string>>(
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

	/*
	// Get last logins
	public static Coroutine GetLastLogins(uint numPlayers, GameDB.ActionOnResult<KeyValue<TimeStamp>[]> func) {
		return GameDB.instance.StartCoroutine(GameDB.MapReduce<KeyValue<TimeStamp>>(
			"AccountToLastLoginDate",
			GameDB.keyValueMapFunction,
			timeStampReduceFunction,
			numPlayers,
			func
		));
	}
	
	// Get last registrations
	public static Coroutine GetLastRegistrations(uint numPlayers, GameDB.ActionOnResult<KeyValue<TimeStamp>[]> func) {
		return GameDB.instance.StartCoroutine(GameDB.MapReduce<KeyValue<TimeStamp>>(
			"AccountToRegistrationDate",
			GameDB.keyValueMapFunction,
			timeStampReduceFunction,
			numPlayers,
			func
		));
	}
	
	// Last logins: Reduce
	private const string timeStampReduceFunction =
		@"
		function(valueList, maxPlayerCount) {
			// Sort
			valueList.sort(function(a, b) {
				return b.val.unixTimeStamp - a.val.unixTimeStamp;
			});
			
			// Shorten
			if(valueList.length > maxPlayerCount)
				valueList.length = maxPlayerCount;
			
			return valueList;
		}
		";
	*/
}