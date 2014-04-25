using UnityEngine;
using System.Collections;

public class ItemInventoryDB : SingletonMonoBehaviour<ItemInventoryDB> {
	// --------------------------------------------------------------------------------
	// AccountToItemInventory
	// --------------------------------------------------------------------------------
	
	// Get item inventory
	public IEnumerator GetItemInventory(string accountId, GameDB.ActionOnResult<ItemInventory> func) {
		yield return StartCoroutine(GameDB.Get<ItemInventory>(
			"AccountToItemInventory",
			accountId,
			func
		));
	}
}
