using UnityEngine;

public static class TraitsDB {
	// --------------------------------------------------------------------------------
	// AccountToTraits
	// --------------------------------------------------------------------------------

	// Get traits
	public static Coroutine GetTraits(string accountId, GameDB.ActionOnResult<Traits> func) {
		return GameDB.Async(GameDB.Get<Traits>(
			"AccountToTraits",
			accountId,
			func
		));
	}

	// Set traits
	public static Coroutine SetTraits(string accountId, Traits traits, GameDB.ActionOnResult<Traits> func = null) {
		return GameDB.Async(GameDB.Set<Traits>(
			"AccountToTraits",
			accountId,
			traits,
			func
		));
	}
}