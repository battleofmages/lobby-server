using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class SettingsDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// AccountToInputSettings
	// --------------------------------------------------------------------------------
	
	// Get input settings
	public IEnumerator GetInputSettings(LobbyPlayer player) {
		yield return StartCoroutine(GameDB.Get<InputSettings>(
			"AccountToInputSettings",
			player.accountId,
			data => {
				if(data == null) {
					Lobby.RPC("ReceiveInputSettingsError", player.peer);
				} else {
					// Send the controls to the player
					Lobby.RPC("ReceiveInputSettings", player.peer, Jboy.Json.WriteObject(data));
				}
			}
		));
	}
	
	// Set input settings
	public IEnumerator SetInputSettings(LobbyPlayer player, InputSettings inputMgr) {
		yield return StartCoroutine(GameDB.Set<InputSettings>(
			"AccountToInputSettings",
			player.accountId,
			inputMgr,
			data => {
				// ...
			}
		));
	}
}
