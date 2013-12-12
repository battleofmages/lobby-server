using uLobby;
using UnityEngine;
using System.Collections;

public class FriendsDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// AccountToFriends
	// --------------------------------------------------------------------------------

	// Get friends
	public IEnumerator GetFriends(LobbyPlayer player) {
		yield return StartCoroutine(GameDB.Get<FriendsList>(
			"AccountToFriends",
			player.accountId,
			data => {
				if(data != null)
					player.friends = data;
				else
					player.friends = new FriendsList();
			}
		));
		
		// Send new friends list
		player.OnFriendsListLoaded();
	}

	// Get friends
	public IEnumerator GetFriends(string accountId, GameDB.ActionOnResult<FriendsList> func) {
		yield return StartCoroutine(GameDB.Get<FriendsList>(
			"AccountToFriends",
			accountId,
			func
		));
	}

	// Set friends
	public IEnumerator SetFriends(string accountId, FriendsList friends, GameDB.ActionOnResult<FriendsList> func = null) {
		yield return StartCoroutine(GameDB.Set<FriendsList>(
			"AccountToFriends",
			accountId,
			friends,
			func
		));
	}
}
