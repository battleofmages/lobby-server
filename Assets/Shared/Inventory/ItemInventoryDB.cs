using UnityEngine;
using System.Collections;

public class ItemInventoryDB : MonoBehaviour {
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
