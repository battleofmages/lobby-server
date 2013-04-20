using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class SettingsServer : MonoBehaviour {
	private SettingsDB settingsDB;
	
	void Start () {
		settingsDB = this.GetComponent<SettingsDB>();
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void ClientInputSettings(string inputSettingsString, LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = LobbyServer.GetLobbyPlayer(info);
		
		//XDebug.Log("Player '" + lobbyPlayer.name + "' sent new input settings");
		InputSettings inputSettings = Jboy.Json.ReadObject<InputSettings>(inputSettingsString);
		StartCoroutine(settingsDB.SetInputSettings(lobbyPlayer, inputSettings));
	}
}
