using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class IPInfoDB : SingletonMonoBehaviour<IPInfoDB> {
	// --------------------------------------------------------------------------------
	// IPToAccounts
	// --------------------------------------------------------------------------------
	
	// Set accounts
	public IEnumerator SetAccounts(string ip, string[] accounts, GameDB.ActionOnResult<string[]> func) {
		yield return StartCoroutine(GameDB.Set<string[]>(
			"IPToAccounts",
			ip,
			accounts,
			func
		));
	}
	
	// Get accounts
	public IEnumerator GetAccounts(string ip, GameDB.ActionOnResult<string[]> func) {
		yield return StartCoroutine(GameDB.Get<string[]>(
			"IPToAccounts",
			ip,
			func
		));
	}
	
	// --------------------------------------------------------------------------------
	// AccountToCountry
	// --------------------------------------------------------------------------------
	
	// Set country
	public IEnumerator SetCountry(string accountId, string countryCode, GameDB.ActionOnResult<string> func) {
		yield return StartCoroutine(GameDB.Set<string>(
			"AccountToCountry",
			accountId,
			countryCode,
			func
		));
	}
	
	// Get country
	public IEnumerator GetCountry(string accountId, GameDB.ActionOnResult<string> func) {
		yield return StartCoroutine(GameDB.Get<string>(
			"AccountToCountry",
			accountId,
			func
		));
	}
}
