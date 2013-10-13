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
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		if(charStats.totalStatPointsUsed > charStats.maxStatPoints) {
			LogManager.General.LogWarning("Detected character stat points hack on player '" +player.name  + "'");
			return;
		}
		
		//LogManager.General.Log("Player '" + player.name + "' sent new character stats " + charStats.ToString());
		StartCoroutine(traitsDB.SetCharacterStats(player, charStats));
	}
}
