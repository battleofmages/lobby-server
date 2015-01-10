using UnityEngine;
using uLobby;
using System.Collections;

public class NameChanger : MonoBehaviour, Initializable {
	// Init
	public void Init() {
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}

	[RPC]
	IEnumerator CheckName(string newName, LobbyMessageInfo info) {
		yield return LobbyGameDB.GetAccountIdByPlayerName(newName, data => {
			if(data != null) {
				Lobby.RPC("NameCheck", info.sender, newName, false);
			} else {
				Lobby.RPC("NameCheck", info.sender, newName, true);
			}
		});
	}
	
	[RPC]
	IEnumerator NameChange(string newName, LobbyMessageInfo info) {
		// Prettify to be safe
		newName = newName.PrettifyPlayerName();
		
		// Validate data
		if(!Validator.playerName.IsMatch(newName)) {
			LogManager.General.LogError("Player name is not valid: '" + newName + "'");
			yield break;
		}
		
		// Check if name exists already
		yield return LobbyGameDB.GetAccountIdByPlayerName(newName, data => {
			if(data != null) {
				Lobby.RPC("NameAlreadyExists", info.sender, newName);
			} else {
				// Get the player
				var player = LobbyPlayer.Get(info);
				
				// Change name
				LogManager.General.Log("Account " + player.account.id + " has requested to change its player name to '" + newName + "'");
				player.account.playerName.value = newName;
			}
		});
	}
}
