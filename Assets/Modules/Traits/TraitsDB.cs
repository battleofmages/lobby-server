using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class TraitsDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// AccountToCharacterStats
	// --------------------------------------------------------------------------------
	
	// Get character stats
	public IEnumerator GetCharacterStats(LobbyPlayer player) {
		yield return StartCoroutine(GameDB.Get<CharacterStats>(
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
	public IEnumerator GetCharacterStats(string accountId, GameDB.ActionOnResult<CharacterStats> func) {
		yield return StartCoroutine(GameDB.Get<CharacterStats>(
			"AccountToCharacterStats",
			accountId,
			func
		));
	}
	
	// Set character stats
	public IEnumerator SetCharacterStats(LobbyPlayer player, CharacterStats charStats) {
		yield return StartCoroutine(GameDB.Set<CharacterStats>(
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
