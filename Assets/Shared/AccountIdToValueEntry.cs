using UnityEngine;
using System.Collections;

[System.Serializable]
public class AccountIdToValueEntry {
	public string accountId;
	public string val;
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		var entry = (AccountIdToValueEntry)instance;
		
		writer.WriteArrayStart();
		writer.WriteString(entry.accountId);
		writer.WriteString(entry.val);
		writer.WriteArrayEnd();
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		var entry = new AccountIdToValueEntry();
		
		reader.ReadArrayStart();
		entry.accountId = reader.ReadString();
		entry.val = reader.ReadString();
		reader.ReadArrayEnd();
		
		return entry;
	}
}
