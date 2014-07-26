using UnityEngine;

public static class LocationsDB {
	// --------------------------------------------------------------------------------
	// AccountToLocation
	// --------------------------------------------------------------------------------
	
	// Set location
	public static Coroutine SetLocation(string accountId, PlayerLocation location, GameDB.ActionOnResult<PlayerLocation> func) {
		return GameDB.instance.StartCoroutine(GameDB.Set<PlayerLocation>(
			"AccountToLocation",
			accountId,
			location,
			func
		));
	}
	
	// Get location
	public static Coroutine GetLocation(string accountId, GameDB.ActionOnResult<PlayerLocation> func) {
		return GameDB.instance.StartCoroutine(GameDB.Get<PlayerLocation>(
			"AccountToLocation",
			accountId,
			func
		));
	}
}