using UnityEngine;
using System.Collections;

[System.Serializable]
public class ArtifactInventory {
	public static int defaultInventoryLimit = 15;
	
	public Inventory[] levels;
	
	public ArtifactInventory() {
		levels = new Inventory[Artifact.maxLevel];
		
		for(int i = 0; i < Artifact.maxLevel; i++) {
			levels[i] = new Inventory(defaultInventoryLimit);
		}
	}
	
	public void AddArtifact(Artifact arti) {
		levels[arti.level].AddItem(arti.id, 1, arti);
	}
	
	public void RemoveArtifact(Artifact arti) {
		levels[arti.level].RemoveItem(arti.id, 1);
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		var inv = (ArtifactInventory)instance;
		
		writer.WriteArrayStart();
		for(var i = 0; i < Artifact.maxLevel; i++) {
			GenericSerializer.WriteJSONClassInstance<Inventory>(writer, inv.levels[i]);
		}
		writer.WriteArrayEnd();
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		var inv = new ArtifactInventory();
		
		reader.ReadArrayStart();
		for(var i = 0; i < Artifact.maxLevel; i++) {
			inv.levels[i] = GenericSerializer.ReadJSONClassInstance<Inventory>(reader);
		}
		reader.ReadArrayEnd();
		
		return inv;
	}
}
