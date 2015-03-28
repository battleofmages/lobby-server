using UnityEngine;
using System.Collections;
using uLobby;
using BoM;
using BoM.Friends;

public class FriendsServer : SingletonMonoBehaviour<FriendsServer> {
	public TextAsset mapFollowers;
	public TextAsset reduceFollowers;

	// Start
	void Start() {
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}

#region RPCs
	[RPC]
	IEnumerator AddFriendToGroup(string friendName, string groupName, LobbyMessageInfo info) {
		string friendAccountId = null;

		yield return LobbyGameDB.GetAccountIdByPlayerName(friendName, data => {
			friendAccountId = data;
		});

		// Error getting account ID?
		if(friendAccountId == null) {
			LogManager.General.LogWarning("Add friend: Player doesn't exist");
			Lobby.RPC("AddFriendError", info.sender, friendName, AddFriendError.PlayerDoesntExist);
			yield break;
		}

		StartCoroutine(AddFriendAccountToGroup(friendAccountId, groupName, info));
	}

	[RPC]
	IEnumerator AddFriendAccountToGroup(string friendAccountId, string groupName, LobbyMessageInfo info) {
		var player = LobbyPlayer.Get(info);
		
		var friends = player.friends;
		
		// Friends list not loaded yet
		if(friends == null) {
			LogManager.General.LogWarning("Add friend: Friends list of " + player + " not loaded yet");
			yield break;
		}
		
		// Find friends group
		var selectedGroup = friends.GetGroupByName(groupName);
		
		// Trying to add yourself?
		if(friendAccountId == player.account.id) {
			LogManager.General.LogWarning("Add friend: Can't add yourself");
			player.RPC("AddFriendError", friendAccountId, AddFriendError.CantAddYourself);
			yield break;
		}
		
		// Already in friends list?
		if(!player.friends.CanAdd(friendAccountId)) {
			LogManager.General.LogWarning("Add friend: Already in friends list");
			player.RPC("AddFriendError", friendAccountId, AddFriendError.AlreadyInFriendsList);
			yield break;
		}

		var friend = new Friend(friendAccountId);
		
		// Log
		LogManager.General.Log(string.Format("{0} added {1} to friends list group '{2}'", player, friend, groupName));
		
		// Add player to the group
		selectedGroup.friends.Add(friend);
		
		// Subscribe to his online status
		//player.SubscribeToOnlineStatus(friend);
		
		// Save friends list in database
		player.account.friendsList.value = friends;
	}

	[RPC]
	IEnumerator RemoveFriendFromGroup(string friendName, string groupName, LobbyMessageInfo info) {
		string friendAccountId = null;

		yield return LobbyGameDB.GetAccountIdByPlayerName(friendName, data => {
			friendAccountId = data;
		});

		// Error getting account ID?
		if(friendAccountId == null) {
			Lobby.RPC("RemoveFriendError", info.sender, friendName, RemoveFriendError.PlayerDoesntExist);
			yield break;
		}

		StartCoroutine(RemoveFriendAccountFromGroup(friendAccountId, groupName, info));
	}

	[RPC]
	IEnumerator RemoveFriendAccountFromGroup(string friendAccountId, string groupName, LobbyMessageInfo info) {
		var player = LobbyPlayer.Get(info);

		// Get friend account
		var friendAccount = PlayerAccount.Get(friendAccountId);
		
		// Find friends group
		var friends = player.friends;
		var selectedGroup = friends.GetGroupByName(groupName);

		// Error getting account ID?
		if(selectedGroup == null) {
			player.RPC("RemoveFriendError", friendAccountId, RemoveFriendError.GroupDoesntExist);
			yield break;
		}

		// Log
		LogManager.General.Log(string.Format("{0} removed {1} from friend list group '{2}'", player, friendAccount, groupName));

		// Remove player from the group
		selectedGroup.friends.RemoveAll(friend => friend.accountId == friendAccountId);

		// Unsubscribe from online status
		friendAccount.onlineStatus.Disconnect(player);
		
		// Save friends list in database
		player.account.friendsList.value = friends;
	}

	/*
	[RPC]
	IEnumerator SetFriendNote(string friendName, string groupName, string note, LobbyMessageInfo info) {
		var player = LobbyPlayer.Get(info);
		LogManager.General.Log(string.Format("'{0}' sets friends list note for '{1}' to '{2}'", player.name, friendName, note));
		
		// Find friends group
		var selectedGroup = player.friends.GetGroupByName(groupName);
		
		// Get account ID
		string friendAccountId = null;
		
		yield return LobbyGameDB.GetAccountIdByPlayerName(friendName, data => {
			friendAccountId = data;
		});
		
		// Error getting account ID?
		if(friendAccountId == null) {
			Lobby.RPC("SetFriendNotePlayerDoesntExistError", info.sender, friendName);
			yield break;
		}
		
		// Remove player from the group
		selectedGroup.friends.Find(friend => friend.accountId == friendAccountId).note = note;
		
		// Save friends list in database
		yield return FriendsDB.SetFriends(
			player.accountId,
			player.friends,
			null
		);
	}
	
	[RPC]
	IEnumerator AddFriendsGroup(string groupName, LobbyMessageInfo info) {
		var player = LobbyPlayer.Get(info);
		LogManager.General.Log(string.Format("'{0}' created new friends group called '{1}'", player.name, groupName));
		
		// Add it
		player.friends.AddGroup(groupName);
		
		// Save friends list in database
		yield return FriendsDB.SetFriends(
			player.accountId,
			player.friends,
			null
		);
	}
	
	[RPC]
	IEnumerator RemoveFriendsGroup(string groupName, LobbyMessageInfo info) {
		var player = LobbyPlayer.Get(info);
		LogManager.General.Log(string.Format("'{0}' removed friends group called '{1}'", player.name, groupName));
		
		// Remove it
		player.friends.RemoveGroup(groupName);
		
		// Save friends list in database
		yield return FriendsDB.SetFriends(
			player.accountId,
			player.friends,
			null
		);
	}
	*/
#endregion
}