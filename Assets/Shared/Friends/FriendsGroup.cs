using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FriendsGroup {
	public string name;
	public List<string> friends;

	public FriendsGroup() {
		name = "";
		friends = new List<string>();
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
