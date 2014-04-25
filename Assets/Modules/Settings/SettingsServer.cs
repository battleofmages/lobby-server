using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class SettingsServer : MonoBehaviour {
	// Start
	void Start () {
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void ClientInputSettings(string inputSettingsString, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		//LogManager.General.Log("Player '" + player.name + "' sent new input settings");
		InputSettings inputSettings = Jboy.Json.ReadObject<InputSettings>(inputSettingsString);
		SettingsDB.SetInputSettings(player, inputSettings);
	}
}
