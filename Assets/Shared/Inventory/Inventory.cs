using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Rarity {
	Common,
	Uncommon,
	Rare,
	Epic,
	Legendary
}

[System.Serializable]
public class Inventory {
	public int itemLimit;
	public List<ItemSlot> itemSlots;
	
	public Inventory() {
		itemLimit = 0;
		itemSlots = null;
	}
	
	public Inventory(int nItemLimit) {
		itemLimit = nItemLimit;
		itemSlots = new List<ItemSlot>(new ItemSlot[nItemLimit]);
	}
	
	public void RemoveItem(int itemId, int count = 1) {
		for(int i = 0; i < itemSlots.Count; i++) {
			if(itemSlots[i] == null)
				continue;
			
			if(itemSlots[i].id == itemId) {
				itemSlots[i].count -= count;
				if(itemSlots[i].count == 0)
					itemSlots[i] = null;
				return;
			}
		}
	}
	
	public void RemoveItemSlot(int slotId) {
		itemSlots[slotId].id = -1;
		itemSlots[slotId].count = 0;
		itemSlots[slotId] = null;
	}
	
	public void AddItem(int itemId, int count, object instance) {
		// Trying to find the item in the inventory and increasing its count
		int freePos = -1;
		for(int i = 0; i < itemSlots.Count; i++) {
			if(itemSlots[i] == null) {
				if(freePos == -1) {
					freePos = i;
				}
			} else {
				if(itemSlots[i].id == itemId) {
					itemSlots[i].count += count;
					return;
				}
			}
		}
		
		// New slot assigned
		if(freePos != -1) {
			itemSlots[freePos] = new ItemSlot(itemId, count, instance);
			return;
		}
		
		// In case inventory is full
		itemSlots.Add(new ItemSlot(itemId, count, instance));
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<Inventory>(writer, (Inventory)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<Inventory>(reader);
	}
}

