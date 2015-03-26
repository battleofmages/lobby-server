using UnityEngine;
using BoM.Friends;

public static class FriendsDB {
	// --------------------------------------------------------------------------------
	// AccountToFriends
	// --------------------------------------------------------------------------------

	// Get friends
	public static Coroutine GetFriends(string accountId, GameDB.ActionOnResult<FriendsList> func) {
		return GameDB.Async(GameDB.Get<FriendsList>(
			"AccountToFriends",
			accountId,
			func
		));
	}

	// Set friends
	public static Coroutine SetFriends(string accountId, FriendsList friends, GameDB.ActionOnResult<FriendsList> func = null) {
		return GameDB.Async(GameDB.Set<FriendsList>(
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
	public static Coroutine GetFollowers(string accountId, GameDB.ActionOnResult<string[]> func = null) {
		return GameDB.Async(GameDB.MapReduce<string>(
			"AccountToFriends",
			GameDB.GetSearchMapFunction("groups"),
			FriendsServer.instance.reduceFollowers.text,
			accountId,
			func
		));
	}
}