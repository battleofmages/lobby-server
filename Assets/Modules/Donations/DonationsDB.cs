using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public static class DonationsDB {
	// --------------------------------------------------------------------------------
	// AccountToPayments
	// --------------------------------------------------------------------------------
	
	// Set payments list
	public static Coroutine SetPaymentsList(string accountId, PaymentsList list, GameDB.ActionOnResult<PaymentsList> func) {
		return GameDB.instance.StartCoroutine(GameDB.Set<PaymentsList>(
			"AccountToPayments",
			accountId,
			list,
			func
		));
	}
	
	// Get payments list
	public static Coroutine GetPaymentsList(string accountId, GameDB.ActionOnResult<PaymentsList> func) {
		return GameDB.instance.StartCoroutine(GameDB.Get<PaymentsList>(
			"AccountToPayments",
			accountId,
			func
		));
	}
}
