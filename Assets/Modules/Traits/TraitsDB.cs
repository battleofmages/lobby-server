using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class TraitsDB : SingletonMonoBehaviour<TraitsDB> {
	// --------------------------------------------------------------------------------
	// AccountToCharacterStats
	// --------------------------------------------------------------------------------
	
	// Get character stats
	public Coroutine GetCharacterStats(LobbyPlayer player) {
		return GameDB.instance.StartCoroutine(GameDB.Get<CharacterStats>(
			"AccountToCharacterStats",
			player.accountId,
			data => {
				if(data == null)
					player.charStats = new CharacterStats();
				else
					player.charStats = data;
				
				Lobby.RPC("ReceiveCharacterStats", player.peer, player.accountId, player.charStats);
			}
		));
	}
	
	// Get character stats
	public Coroutine GetCharacterStats(string accountId, GameDB.ActionOnResult<CharacterStats> func) {
		return GameDB.instance.StartCoroutine(GameDB.Get<CharacterStats>(
			"AccountToCharacterStats",
			accountId,
			func
		));
	}
	
	// Set character stats
	public Coroutine SetCharacterStats(LobbyPlayer player, CharacterStats charStats) {
		return GameDB.instance.StartCoroutine(GameDB.Set<CharacterStats>(
			"AccountToCharacterStats",
			player.accountId,
			charStats,
			data => {
				if(data == null)
					Lobby.RPC("CharacterStatsSaveError", player.peer);
				else
					player.charStats = data;
			}
		));
	}
}
