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
	
	// --------------------------------------------------------------------------------
	// MapReduce
	// --------------------------------------------------------------------------------
	
	// Get followers
	public IEnumerator GetFollowers(LobbyPlayer player) {
		yield return StartCoroutine(GameDB.MapReduce<string>(
			"AccountToFriends",
			GameDB.GetSearchMapFunction("groups"),
			followersReduceFunction,
			player.accountId,
			data => {
				player.followers = data;
			}
		));
		
		// Send new followers list
		player.OnFollowersListLoaded();
	}
	
	// Reduce
	private static string followersReduceFunction = 
	@"
	function(valueList, idToFind) {
		var length = valueList.length;
		var element = null;
		var groups = null;
		var result = [];
		
		for(var i = 0; i < length; i++) {
			element = valueList[i];
			groups = element[1];
			
			for(var h = 0; h < groups.length; h++) {
				for(var j = 0; j < groups[h].friends.length; j++) {
					if(groups[h].friends[j].accountId == idToFind) {
						result.push(element[0]);
					}
				}
			}
		}
		
		return result;
	}
	";
}
