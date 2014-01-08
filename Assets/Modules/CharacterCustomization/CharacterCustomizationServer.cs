using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class CharacterCustomizationServer : MonoBehaviour {
	private CharacterCustomizationDB characterCustomizationDB;

	// Start
	void Start() {
		characterCustomizationDB = this.GetComponent<CharacterCustomizationDB>();
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void ClientCharacterCustomization(CharacterCustomization custom, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		LogManager.General.Log(string.Format("Account '{0}' sent character customization!", player.account.name));
		
		StartCoroutine(characterCustomizationDB.SetCharacterCustomization(
			player.accountId,
			custom,
			data => {
				if(data != null) {
					player.custom = data;
					Lobby.RPC("ReceiveCharacterCustomization", player.peer, player.accountId, player.custom);
				}
			}
		));
	}
}
