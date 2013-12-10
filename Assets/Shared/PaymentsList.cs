using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PaymentsList : JsonSerializable<PaymentsList> {
	public double balance;
	public List<string> payments;
}
