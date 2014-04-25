using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class AccessLevelsDB : SingletonMonoBehaviour<AccessLevelsDB> {
	// --------------------------------------------------------------------------------
	// AccountToAccessLevel
	// --------------------------------------------------------------------------------
	
	// Get access level
	public IEnumerator GetAccessLevel(LobbyPlayer player) {
#if UNITY_EDITOR
		// TODO: Temporary
		if(player.accountId == "gXmEbeSp")
			player.accessLevel = AccessLevel.Admin;
		else
#endif
			yield return StartCoroutine(GameDB.Get<byte>(
				"AccountToAccessLevel",
				player.accountId,
				data => {
					player.accessLevel = (AccessLevel)data;
				}
			));
		
		// Send access level
		Lobby.RPC("ReceiveAccessLevel", player.peer, player.accountId, (byte)player.accessLevel);
		
		// Staff info
		//if(player.accessLevel >= AccessLevel.VIP)
		//	LobbyServer.instance.SendStaffInfo(player);
	}
	
	// Set character stats
	public IEnumerator SetAccessLevel(LobbyPlayer player, AccessLevel level) {
		yield return StartCoroutine(GameDB.Set<byte>(
			"AccountToAccessLevel",
			player.accountId,
			(byte)level,
			data => {
				player.accessLevel = (AccessLevel)level;
			}
		));
	}
}
