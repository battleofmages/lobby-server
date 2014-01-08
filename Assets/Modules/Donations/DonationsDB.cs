using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class DonationsDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// AccountToPayments
	// --------------------------------------------------------------------------------
	
	// Set payments list
	public IEnumerator SetPaymentsList(string accountId, PaymentsList list, GameDB.ActionOnResult<PaymentsList> func) {
		yield return StartCoroutine(GameDB.Set<PaymentsList>(
			"AccountToPayments",
			accountId,
			list,
			func
		));
	}
	
	// Get payments list
	public IEnumerator GetPaymentsList(string accountId, GameDB.ActionOnResult<PaymentsList> func) {
		yield return StartCoroutine(GameDB.Get<PaymentsList>(
			"AccountToPayments",
			accountId,
			func
		));
	}
}
