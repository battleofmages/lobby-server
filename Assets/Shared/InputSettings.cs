using UnityEngine;
using System.Collections;

public class InputSettings {
	public InputControl[] controls;
	
	public InputSettings() {
		controls = new InputControl[0];
	}
	
	public InputSettings(InputManager inputMgr) {
		controls = inputMgr.controls;
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<InputSettings>(writer, (InputSettings)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<InputSettings>(reader);
	}
}
