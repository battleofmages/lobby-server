using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class CharacterCustomizationDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// AccountToCharacterCustomization
	// --------------------------------------------------------------------------------
	
	// Get character customization
	public IEnumerator GetCharacterCustomization(string accountId, GameDB.ActionOnResult<CharacterCustomization> func) {
		yield return StartCoroutine(GameDB.Get<CharacterCustomization>(
			"AccountToCharacterCustomization",
			accountId,
			func
		));
	}
	
	// Set character customization
	public IEnumerator SetCharacterCustomization(string accountId, CharacterCustomization custom, GameDB.ActionOnResult<CharacterCustomization> func = null) {
		yield return StartCoroutine(GameDB.Set<CharacterCustomization>(
			"AccountToCharacterCustomization",
			accountId,
			custom,
			func
		));
	}
}
