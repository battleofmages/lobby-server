using UnityEngine;
using System.Collections;

[System.Serializable]
public class TimeStamp {
	public double unixTimeStamp;
	public string readableDateTime;
	
	public TimeStamp() {
		var dt = System.DateTime.UtcNow;
		unixTimeStamp = DateTimeToUnixTimeStamp(dt);
		readableDateTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
	}
	
	public TimeStamp(System.DateTime dt) {
		unixTimeStamp = DateTimeToUnixTimeStamp(dt);
		readableDateTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<TimeStamp>(writer, (TimeStamp)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<TimeStamp>(reader);
	}
	
	// Unix Timestamp -> DateTime
	public static System.DateTime UnixTimeStampToDateTime(double nUnixTimeStamp) {
		// Unix timestamp is seconds past epoch
		System.DateTime dtDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
		dtDateTime = dtDateTime.AddSeconds(nUnixTimeStamp).ToUniversalTime();
		return dtDateTime;
	}
	
	// DateTime -> Unix Timestamp
	public static double DateTimeToUnixTimeStamp(System.DateTime date) {
		System.TimeSpan span = (date - new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc));
		return span.TotalSeconds;
	}
}
