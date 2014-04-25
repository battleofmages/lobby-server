using UnityEngine;
using uLobby;

public class TraitsServer : MonoBehaviour {
	// Start
	void Start() {
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void ClientCharacterStats(CharacterStats charStats, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		if(!charStats.valid) {
			LogManager.General.LogWarning("Detected character stat points hack on player '" +player.name  + "'");
			return;
		}
		
		//LogManager.General.Log("Player '" + player.name + "' sent new character stats " + charStats.ToString());
		TraitsDB.SetCharacterStats(player, charStats);
	}
}
