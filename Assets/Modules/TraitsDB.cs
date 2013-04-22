using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class TraitsDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// AccountToCharacterStats
	// --------------------------------------------------------------------------------
	
	// Get character stats
	public IEnumerator GetCharacterStats(LobbyPlayer lobbyPlayer) {
		yield return StartCoroutine(GameDB.Get<CharacterStats>(
			"AccountToCharacterStats",
			lobbyPlayer.accountId,
			data => {
				if(data == null)
					lobbyPlayer.charStats = new CharacterStats();
				else
					lobbyPlayer.charStats = data;
			}
		));
		
		Lobby.RPC("ReceiveCharacterStats", lobbyPlayer.peer, lobbyPlayer.charStats);
	}
	
	// Set character stats
	public IEnumerator SetCharacterStats(LobbyPlayer lobbyPlayer, CharacterStats charStats) {
		yield return StartCoroutine(GameDB.Set<CharacterStats>(
			"AccountToCharacterStats",
			lobbyPlayer.accountId,
			charStats,
			data => {
				if(data == null)
					Lobby.RPC("CharacterStatsSaveError", lobbyPlayer.peer);
				else
					lobbyPlayer.charStats = data;
			}
		));
	}
}
