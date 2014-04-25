using UnityEngine;
using uLobby;

public class CharacterCustomizationServer : MonoBehaviour {
	// Start
	void Start() {
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
		
		CharacterCustomizationDB.SetCharacterCustomization(
			player.accountId,
			custom,
			data => {
				if(data != null) {
					player.custom = data;
					Lobby.RPC("ReceiveCharacterCustomization", player.peer, player.accountId, player.custom);
				}
			}
		);
	}
}
