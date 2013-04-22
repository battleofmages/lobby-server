using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class SettingsDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// AccountToInputSettings
	// --------------------------------------------------------------------------------
	
	// Get input settings
	public IEnumerator GetInputSettings(LobbyPlayer lobbyPlayer) {
		yield return StartCoroutine(GameDB.Get<InputSettings>(
			"AccountToInputSettings",
			lobbyPlayer.accountId,
			data => {
				if(data == null) {
					Lobby.RPC("ReceiveInputSettingsError", lobbyPlayer.peer);
				} else {
					// Send the controls to the player
					Lobby.RPC("ReceiveInputSettings", lobbyPlayer.peer, Jboy.Json.WriteObject(data));
				}
			}
		));
	}
	
	// Set input settings
	public IEnumerator SetInputSettings(LobbyPlayer lobbyPlayer, InputSettings inputMgr) {
		yield return StartCoroutine(GameDB.Set<InputSettings>(
			"AccountToInputSettings",
			lobbyPlayer.accountId,
			inputMgr,
			data => {
				// ...
			}
		));
	}
}
