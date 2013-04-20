using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class TraitsServer : MonoBehaviour {
	private TraitsDB traitsDB;
	
	void Start () {
		traitsDB = this.GetComponent<TraitsDB>();
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void ClientCharacterStats(CharacterStats charStats, LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = LobbyServer.GetLobbyPlayer(info);
		
		if(charStats.totalStatPointsUsed > charStats.maxStatPoints) {
			XDebug.LogWarning("Detected character stat points hack on player '" +lobbyPlayer.name  + "'");
			return;
		}
		
		//XDebug.Log("Player '" + lobbyPlayer.name + "' sent new character stats " + charStats.ToString());
		StartCoroutine(traitsDB.SetCharacterStats(lobbyPlayer, charStats));
	}
}
