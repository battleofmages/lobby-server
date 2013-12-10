using UnityEngine;
using System.Collections;
using uLobby;

public class FriendsServer : MonoBehaviour {
	private FriendsDB friendsDB;

	// Start
	void Start() {
		friendsDB = this.GetComponent<FriendsDB>();
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}

	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------

	[RPC]
	void ClientFriendsList(FriendsList friendsList, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		LogManager.General.Log(string.Format("Account '{0}' sent new friends list!", player.account.name));
		
		StartCoroutine(friendsDB.SetFriends(
			player.accountId,
			friendsList,
			data => {
				if(data != null) {
					player.friends = data;
					//Lobby.RPC("ReceiveFriendsList", player.peer, player.accountId, player.friends);
				}
			}
		));
	}
}
