using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FriendsList : JsonSerializable<FriendsList> {
	public List<FriendsGroup> groups;

	// Constructor
	public FriendsList() {
		groups = new List<FriendsGroup>();
		groups.Add(new FriendsGroup("General"));
	}
}
