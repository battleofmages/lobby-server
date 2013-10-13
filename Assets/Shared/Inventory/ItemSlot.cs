using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ItemSlot {
	public int id;
	public int count;
	public object instance;
	
	public ItemSlot() {
		id = -1;
		count = 0;
		instance = null;
	}
	
	public ItemSlot(int nID, int nCount = 1) {
		id = nID;
		count = nCount;
		instance = null;
	}
	
	public ItemSlot(int nID, int nCount, object nInstance) {
		id = nID;
		count = nCount;
		instance = nInstance;
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		var fieldExclude = new HashSet<string>() {
			"instance",
		};
		GenericSerializer.WriteJSONClassInstance<ItemSlot>(writer, (ItemSlot)instance, null, fieldExclude);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<ItemSlot>(reader);
	}
}
