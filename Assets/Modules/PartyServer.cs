using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class PartyServer : MonoBehaviour {
	// Start
	void Start () {
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void InviteToParty(string invitedPlayerName, LobbyMessageInfo info) {
		var player = LobbyServer.GetLobbyPlayer(info);
		
		LogManager.General.Log(string.Format("'{0}' sent a party invitation to '{1}'", player.name, invitedPlayerName));
	}
}
