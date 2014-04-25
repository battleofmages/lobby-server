using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class DonationsDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// AccountToPayments
	// --------------------------------------------------------------------------------
	
	// Set payments list
	public Coroutine SetPaymentsList(string accountId, PaymentsList list, GameDB.ActionOnResult<PaymentsList> func) {
		return GameDB.instance.StartCoroutine(GameDB.Set<PaymentsList>(
			"AccountToPayments",
			accountId,
			list,
			func
		));
	}
	
	// Get payments list
	public Coroutine GetPaymentsList(string accountId, GameDB.ActionOnResult<PaymentsList> func) {
		return GameDB.instance.StartCoroutine(GameDB.Get<PaymentsList>(
			"AccountToPayments",
			accountId,
			func
		));
	}
}
