public abstract class JsonSerializable<T> where T : new() {
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<T>(writer, (T)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<T>(reader);
	}
}