using UnityEngine;
using System.Collections;

[System.Serializable]
public class ArtifactTree {
	public ArtifactSlot[][] slots;
	
	public ArtifactTree() {
		slots = new ArtifactSlot[5][];
		
		for(byte i = 0; i < Artifact.maxLevel; i++) {
			int numSlots = Artifact.maxLevel - i;
			slots[i] = new ArtifactSlot[numSlots];
			
			for(int slotIndex = 0; slotIndex < numSlots; slotIndex++) {
				slots[i][slotIndex] = new ArtifactSlot(i);
			}
		}
	}
	
	public CharacterStats charStats {
		get {
			CharacterStats stats = new CharacterStats(0);
			
			foreach(var slotLevel in this.slots) {
				foreach(var slot in slotLevel) {
					if(slot.artifact != null) {
						stats.ApplyOffset(slot.artifact.charStats);
					}
				}
			}
			
			return stats;
		}
	}
	
	public bool AddArtifact(int itemId) {
		var arti = new Artifact(itemId);
		var slotLevel = slots[arti.level];
		
		for(int i = 0; i < slotLevel.Length; i++) {
			if(slotLevel[i].artifact == null) {
				slotLevel[i].artifact = arti;
				return true;
			}
		}
		
		return false;
	}
	
	public static ArtifactTree GetStarterArtifactTree() {
		var tree = new ArtifactTree();
		tree.BuildStarterArtifacts();
		return tree;
	}
	
	public void BuildStarterArtifacts() {
		bool otherHalf = false;
		
		foreach(var slotLevel in slots) {
			foreach(var slot in slotLevel) {
				slot.artifact = new Artifact(slot.requiredLevel);
				slot.artifact.stats[0] = otherHalf ? Artifact.Stat.Attack : Artifact.Stat.Defense;
				slot.artifact.stats[1] = otherHalf ? Artifact.Stat.Energy : Artifact.Stat.AttackSpeed;
				slot.artifact.stats[2] = otherHalf ? Artifact.Stat.MoveSpeed : Artifact.Stat.CooldownReduction;
				
				otherHalf = !otherHalf;
			}
		}
	}
	
	public void Randomize() {
		foreach(var slotLevel in slots) {
			foreach(var slot in slotLevel) {
				slot.artifact = new Artifact(slot.requiredLevel);
			}
		}
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		writer.WriteArrayStart();
		
		var tree = (ArtifactTree)instance;
		foreach(var slotLevel in tree.slots) {
			writer.WriteArrayStart();
			
			for(int i = 0; i < slotLevel.Length; i++) {
				if(slotLevel[i].artifact != null)
					writer.WriteNumber(slotLevel[i].artifact.id);
				else
					writer.WriteNumber(-1);
			}
			
			writer.WriteArrayEnd();
		}
		
		writer.WriteArrayEnd();
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		reader.ReadArrayStart();
		
		var tree = new ArtifactTree();
		for(int i = 0; i < tree.slots.Length; i++) {
			int numSlots = Artifact.maxLevel - i;
			var slotLevel = tree.slots[i];
			reader.ReadArrayStart();
			
			for(int slotIndex = 0; slotIndex < numSlots; slotIndex++) {
				int itemId = (int)reader.ReadNumber();
				slotLevel[slotIndex] = new ArtifactSlot((byte)i);
				
				if(itemId != -1)
					slotLevel[slotIndex].artifact = new Artifact(itemId);
			}
			
			reader.ReadArrayEnd();
		}
		
		reader.ReadArrayEnd();
		return tree;
	}
}
