using UnityEngine;

public class FriendsDB : SingletonMonoBehaviour<FriendsDB> {
	// --------------------------------------------------------------------------------
	// AccountToFriends
	// --------------------------------------------------------------------------------

	// Get friends
	public Coroutine GetFriends(LobbyPlayer player) {
		return GameDB.instance.StartCoroutine(GameDB.Get<FriendsList>(
			"AccountToFriends",
			player.accountId,
			data => {
				if(data != null)
					player.friends = data;
				else
					player.friends = new FriendsList();
				
				// Send new friends list
				player.OnFriendsListLoaded();
			}
		));
	}

	// Get friends
	public Coroutine GetFriends(string accountId, GameDB.ActionOnResult<FriendsList> func) {
		return GameDB.instance.StartCoroutine(GameDB.Get<FriendsList>(
			"AccountToFriends",
			accountId,
			func
		));
	}

	// Set friends
	public Coroutine SetFriends(string accountId, FriendsList friends, GameDB.ActionOnResult<FriendsList> func = null) {
		return GameDB.instance.StartCoroutine(GameDB.Set<FriendsList>(
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
	public Coroutine GetFollowers(LobbyPlayer player) {
		return GameDB.instance.StartCoroutine(GameDB.MapReduce<string>(
			"AccountToFriends",
			GameDB.GetSearchMapFunction("groups"),
			followersReduceFunction,
			player.accountId,
			data => {
				player.followers = data;
			
				// Send new followers list
				player.OnFollowersListLoaded();
			}
		));
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
