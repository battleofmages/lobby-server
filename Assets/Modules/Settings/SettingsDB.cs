using UnityEngine;
using uLobby;

public static class SettingsDB {
	// --------------------------------------------------------------------------------
	// AccountToInputSettings
	// --------------------------------------------------------------------------------
	
	// Get input settings
	public static Coroutine GetInputSettings(LobbyPlayer player) {
		return GameDB.instance.StartCoroutine(GameDB.Get<InputSettings>(
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
	public static Coroutine SetInputSettings(LobbyPlayer player, InputSettings inputMgr) {
		return GameDB.instance.StartCoroutine(GameDB.Set<InputSettings>(
			"AccountToInputSettings",
			player.accountId,
			inputMgr,
			data => {
				// ...
			}
		));
	}
}
