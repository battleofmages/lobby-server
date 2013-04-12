using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericSerializer {
	// Write a single value in JSON
	public static void WriteJSONValue(Jboy.JsonWriter writer, object val) {
		if(val is int || val is KeyCode) {
			writer.WriteNumber((double)((int)val));
		} else if(val is long) {
			writer.WriteNumber((double)((long)val));
		} else if(val is double) {
			writer.WriteNumber((double)val);
		} else if(val is PlayerQueueStats) {
			GenericSerializer.WriteJSONClassInstance<PlayerQueueStats>(writer, (PlayerQueueStats)val);
		} else if(val is PlayerQueueStats[]) {
			writer.WriteArrayStart();
			
			PlayerQueueStats[] valArray = (PlayerQueueStats[])val;
			for(int i = 0; i < QueueSettings.queueCount; i++) {
				GenericSerializer.WriteJSONClassInstance<PlayerQueueStats>(writer, valArray[i]);
			}
			
			writer.WriteArrayEnd();
		} else {
			Jboy.Json.WriteObject(val, writer);
		}
	}
	
	// Writes all fields of a class instance
	public static void WriteJSONClassInstance<T>(Jboy.JsonWriter writer, T instance, HashSet<string> fieldFilter = null) {
		// Type pointer
		Type type = typeof(T);
		
		// Obtain all fields
		FieldInfo[] fields = type.GetFields();
		
		// Loop through all fields
		writer.WriteObjectStart();
		
		foreach(var field in fields) {
			// Get property name and value
			string name = field.Name;
			
			if(fieldFilter != null && !fieldFilter.Contains(name))
				continue;
			
			object val = field.GetValue(instance);
			
			//XDebug.Log("Writing '" + name + "'"); // with value '" + ((double)val).ToString() + "'");
			//XDebug.Log("ValueType " + val.GetType().ToString());
			//XDebug.Log("Value " + val.ToString());
			
			// Write them to the JSON stream
			writer.WritePropertyName(name);
			GenericSerializer.WriteJSONValue(writer, val);
		}
		
		writer.WriteObjectEnd();
	}
	
	// Read a single value from JSON
	public static object ReadJSONValue(Jboy.JsonReader reader, FieldInfo field) {
		if(field.FieldType == typeof(int)) {
			return (int)(reader.ReadNumber());
		} else if(field.FieldType == typeof(long)) {
			return (long)(reader.ReadNumber());
		} else if(field.FieldType == typeof(double)) {
			return reader.ReadNumber();
		} else if(field.FieldType == typeof(KeyCode)) {
			return (KeyCode)(reader.ReadNumber());
		} else if(field.FieldType == typeof(string)) {
			return reader.ReadString();
		}else if(field.FieldType == typeof(PlayerQueueStats)) {
			return GenericSerializer.ReadJSONClassInstance<PlayerQueueStats>(reader);
		} else if(field.FieldType == typeof(PlayerQueueStats[])) {
			reader.ReadArrayStart();
			
			PlayerQueueStats[] valArray = new PlayerQueueStats[QueueSettings.queueCount];
			for(int i = 0; i < QueueSettings.queueCount; i++) {
				valArray[i] = GenericSerializer.ReadJSONClassInstance<PlayerQueueStats>(reader);
			}
			
			reader.ReadArrayEnd();
			
			return valArray;
		} else if(field.FieldType == typeof(InputControl[])) {
			reader.ReadArrayStart();
			
			List<InputControl> valList = new List<InputControl>();
			while(true) {
				try {
					valList.Add(GenericSerializer.ReadJSONClassInstance<InputControl>(reader));
				} catch {
					break;
				}
			}
			
			reader.ReadArrayEnd();
			
			return valList.ToArray();
		} else {
			Debug.LogError("Unknown field type for GenericSerializer.ReadJSONValue: " + field.FieldType);
			return (int)(reader.ReadNumber());
		}
	}
	
	// Read a new class instance from JSON
	public static T ReadJSONClassInstance<T>(Jboy.JsonReader reader) where T : new() {
		T instance = new T();
		
		reader.ReadObjectStart();
		
		string propName;
		bool success = true;
		var typeInfo = typeof(T);
		
		while(true) {
			success = reader.TryReadPropertyName(out propName);
			
			if(success) {
				var field = typeInfo.GetField(propName);
				field.SetValue(instance, GenericSerializer.ReadJSONValue(reader, field));
			} else {
				break;
			}
		}
		
		reader.ReadObjectEnd();
		
		return instance;
	}
}
