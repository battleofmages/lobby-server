using UnityEngine;
using System.Collections;

public class ItemInventoryDB : SingletonMonoBehaviour<ItemInventoryDB> {
	// --------------------------------------------------------------------------------
	// AccountToItemInventory
	// --------------------------------------------------------------------------------
	
	// Get item inventory
	public Coroutine GetItemInventory(string accountId, GameDB.ActionOnResult<ItemInventory> func) {
		return GameDB.instance.StartCoroutine(GameDB.Get<ItemInventory>(
			"AccountToItemInventory",
			accountId,
			func
		));
	}
}
