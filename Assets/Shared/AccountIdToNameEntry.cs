using UnityEngine;
using System.Collections;

[System.Serializable]
public class AccountIdToNameEntry {
	public string accountId;
	public string name;
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		var entry = (AccountIdToNameEntry)instance;
		
		writer.WriteArrayStart();
		writer.WriteString(entry.accountId);
		writer.WriteString(entry.name);
		writer.WriteArrayEnd();
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		var entry = new AccountIdToNameEntry();
		
		reader.ReadArrayStart();
		entry.accountId = reader.ReadString();
		entry.name = reader.ReadString();
		reader.ReadArrayEnd();
		
		return entry;
	}
}
