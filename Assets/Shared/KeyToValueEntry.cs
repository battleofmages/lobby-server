using UnityEngine;
using System.Collections;

[System.Serializable]
public class KeyToValueEntry {
	public string key;
	public string val;
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		var entry = (KeyToValueEntry)instance;
		
		writer.WriteArrayStart();
		writer.WriteString(entry.key);
		writer.WriteString(entry.val);
		writer.WriteArrayEnd();
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		var entry = new KeyToValueEntry();
		
		reader.ReadArrayStart();
		entry.key = reader.ReadString();
		entry.val = reader.ReadString();
		reader.ReadArrayEnd();
		
		return entry;
	}
}
