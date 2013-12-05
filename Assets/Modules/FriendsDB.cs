using UnityEngine;
using System.Collections;

public class FriendsDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// AccountToFriends
	// --------------------------------------------------------------------------------

	// Get friends
	public IEnumerator GetFriends(string accountId, GameDB.ActionOnResult<FriendsList> func) {
		yield return StartCoroutine(GameDB.Get<FriendsList>(
			"AccountToFriends",
			accountId,
			func
		));
	}
}
