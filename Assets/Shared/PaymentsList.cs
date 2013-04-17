using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PaymentsList {
	public double balance;
	public List<string> payments;
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<PaymentsList>(writer, (PaymentsList)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<PaymentsList>(reader);
	}
}
