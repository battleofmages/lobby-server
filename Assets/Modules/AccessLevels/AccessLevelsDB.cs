using UnityEngine;
using uLobby;

public static class AccessLevelsDB {
	// --------------------------------------------------------------------------------
	// AccountToAccessLevel
	// --------------------------------------------------------------------------------
	
	// Get access level
	public static Coroutine GetAccessLevel(LobbyPlayer player) {
		return GameDB.instance.StartCoroutine(GameDB.Get<byte>(
			"AccountToAccessLevel",
			player.accountId,
			data => {
				player.accessLevel = (AccessLevel)data;
				
				// Send access level
				Lobby.RPC("ReceiveAccessLevel", player.peer, player.accountId, (byte)player.accessLevel);
			}
		));
		
		// Staff info
		//if(player.accessLevel >= AccessLevel.VIP)
		//	LobbyServer.instance.SendStaffInfo(player);
	}
	
	// Set access level
	public static Coroutine SetAccessLevel(LobbyPlayer player, AccessLevel level) {
		return GameDB.instance.StartCoroutine(GameDB.Set<byte>(
			"AccountToAccessLevel",
			player.accountId,
			(byte)level,
			data => {
				player.accessLevel = (AccessLevel)level;
			}
		));
	}
}
