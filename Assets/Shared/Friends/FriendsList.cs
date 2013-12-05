using UnityEngine;
using System.Collections.Generic;

public class FriendsList {
	public List<FriendsGroup> groups;

	public FriendsList() {
		groups = new List<FriendsGroup>();
	}

	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<FriendsGroup>(writer, (FriendsGroup)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<FriendsGroup>(reader);
	}
}
