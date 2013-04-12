using UnityEngine;
using System.Collections;

[System.Serializable]
public class GuildMember {
	public string accountId;
	public string name;
	public int rank;
	public TimeStamp joinDate;
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<GuildMember>(writer, (GuildMember)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<GuildMember>(reader);
	}
}

[System.Serializable]
public class Guild {
	public string name;
	public double level;
	public GuildMember[] members;
	public PlayerQueueStats stats;
	public TimeStamp creationDate;
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<Guild>(writer, (Guild)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<Guild>(reader);
	}
}
