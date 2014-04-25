using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class IPInfoDB : SingletonMonoBehaviour<IPInfoDB> {
	// --------------------------------------------------------------------------------
	// IPToAccounts
	// --------------------------------------------------------------------------------
	
	// Set accounts
	public Coroutine SetAccounts(string ip, string[] accounts, GameDB.ActionOnResult<string[]> func) {
		return GameDB.instance.StartCoroutine(GameDB.Set<string[]>(
			"IPToAccounts",
			ip,
			accounts,
			func
		));
	}
	
	// Get accounts
	public Coroutine GetAccounts(string ip, GameDB.ActionOnResult<string[]> func) {
		return GameDB.instance.StartCoroutine(GameDB.Get<string[]>(
			"IPToAccounts",
			ip,
			func
		));
	}
	
	// --------------------------------------------------------------------------------
	// AccountToCountry
	// --------------------------------------------------------------------------------
	
	// Set country
	public Coroutine SetCountry(string accountId, string countryCode, GameDB.ActionOnResult<string> func) {
		return GameDB.instance.StartCoroutine(GameDB.Set<string>(
			"AccountToCountry",
			accountId,
			countryCode,
			func
		));
	}
	
	// Get country
	public Coroutine GetCountry(string accountId, GameDB.ActionOnResult<string> func) {
		return GameDB.instance.StartCoroutine(GameDB.Get<string>(
			"AccountToCountry",
			accountId,
			func
		));
	}
}
