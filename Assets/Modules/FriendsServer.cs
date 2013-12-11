using UnityEngine;
using System.Collections;
using System.Linq;
using uLobby;

public class FriendsServer : MonoBehaviour {
	private FriendsDB friendsDB;
	private LobbyGameDB lobbyGameDB;

	// Start
	void Start() {
		friendsDB = GetComponent<FriendsDB>();
		lobbyGameDB = GetComponent<LobbyGameDB>();
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}

	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	IEnumerator ClientAddFriend(string friendName, string groupName, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		LogManager.General.Log(string.Format("Player '{0}' wants to add player '{1}' to friend list group '{2}'!", player.account.name, friendName, groupName));
		
		// Find friends group
		var selectedGroup = player.friends.groups.Find(grp => grp.name == groupName);
		
		// Get account ID
		string playerAccountId = null;
		
		yield return StartCoroutine(lobbyGameDB.GetAccountIdByPlayerName(friendName, data => {
			playerAccountId = data;
		}));
		
		// Error getting account ID?
		if(playerAccountId == null) {
			Lobby.RPC("FriendAddPlayerDoesntExistError", info.sender, friendName);
			yield break;
		}
		
		// Add player to the group
		selectedGroup.friends.Add(new Friend(playerAccountId));
		
		// Set friends list
		yield return StartCoroutine(friendsDB.SetFriends(
			player.accountId,
			player.friends,
			null
		));
		
		// Send new friends list
		Lobby.RPC("ReceiveFriendsList", player.peer, player.accountId, Jboy.Json.WriteObject(player.friends));
	}
	
	/*[RPC]
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
	}*/
}
