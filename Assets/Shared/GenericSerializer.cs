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
		//} else if(val is PlayerQueueStats) {
		//	GenericSerializer.WriteJSONClassInstance<PlayerQueueStats>(writer, (PlayerQueueStats)val);
		/*} else if(val is PlayerQueueStats[]) {
			writer.WriteArrayStart();
			
			PlayerQueueStats[] valArray = (PlayerQueueStats[])val;
			for(int i = 0; i < QueueSettings.queueCount; i++) {
				GenericSerializer.WriteJSONClassInstance<PlayerQueueStats>(writer, valArray[i]);
			}
			
			writer.WriteArrayEnd();*/
		} else {
			Jboy.Json.WriteObject(val, writer);
		}
	}
	
	// Writes all fields of a class instance
	public static void WriteJSONClassInstance<T>(Jboy.JsonWriter writer, T instance, HashSet<string> fieldFilter = null, HashSet<string> fieldExclude = null) {
		if(instance == null) {
			writer.WriteNull();
			return;
		}
		
		// Type pointer
		Type type = typeof(T);
		
		// Obtain all fields
		FieldInfo[] fields = type.GetFields();
		
		// Loop through all fields
		writer.WriteObjectStart();
		
		foreach(var field in fields) {
			if(field.IsStatic)
				continue;
			
			// Get property name and value
			string name = field.Name;
			
			if(fieldFilter != null && !fieldFilter.Contains(name))
				continue;
			
			if(fieldExclude != null && fieldExclude.Contains(name))
				continue;
			
			object val = field.GetValue(instance);
			
			//LogManager.General.Log("Writing '" + name + "'"); // with value '" + ((double)val).ToString() + "'");
			//LogManager.General.Log("ValueType " + val.GetType().ToString());
			//LogManager.General.Log("Value " + val.ToString());
			
			// Write them to the JSON stream
			writer.WritePropertyName(name);
			GenericSerializer.WriteJSONValue(writer, val);
		}
		
		writer.WriteObjectEnd();
	}
	
	// Read a single value from JSON
	public static object ReadJSONValue(Jboy.JsonReader reader, FieldInfo field) {
		var fieldType = field.FieldType;
		
		if(fieldType == typeof(int)) {
			return (int)(reader.ReadNumber());
		} else if(fieldType == typeof(long)) {
			return (long)(reader.ReadNumber());
		} else if(fieldType == typeof(byte)) {
			return (byte)(reader.ReadNumber());
		} else if(fieldType == typeof(double)) {
			return reader.ReadNumber();
		} else if(fieldType == typeof(float)) {
			return (float)reader.ReadNumber();
		} else if(fieldType == typeof(KeyCode)) {
			return (KeyCode)(reader.ReadNumber());
		} else if(fieldType == typeof(string)) {
			return reader.ReadString();
		} else if(fieldType == typeof(int[])) {
			return Jboy.Json.ReadObject<int[]>(reader);
		} else if(fieldType == typeof(Color)) {
			return GenericSerializer.ColorJsonDeserializer(reader);
		} else if(fieldType == typeof(PlayerQueueStats)) {
			return GenericSerializer.ReadJSONClassInstance<PlayerQueueStats>(reader);
		} else if(fieldType == typeof(PlayerQueueStats[])) {
			reader.ReadArrayStart();
			
			PlayerQueueStats[] valArray = new PlayerQueueStats[QueueSettings.queueCount];
			for(int i = 0; i < QueueSettings.queueCount; i++) {
				valArray[i] = GenericSerializer.ReadJSONClassInstance<PlayerQueueStats>(reader);
			}
			
			reader.ReadArrayEnd();
			
			return valArray;
		} else if(fieldType == typeof(InputControl[])) {
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
		} else if(fieldType == typeof(Artifact)) {
			return Jboy.Json.ReadObject<Artifact>(reader);
		} else if(fieldType == typeof(ArtifactSlot)) {
			return Jboy.Json.ReadObject<ArtifactSlot>(reader);
		} else if(fieldType == typeof(ArtifactTree)) {
			return Jboy.Json.ReadObject<ArtifactTree>(reader);
		} else if(fieldType == typeof(ArtifactInventory)) {
			return Jboy.Json.ReadObject<ArtifactInventory>(reader);
		} else if(fieldType == typeof(List<ItemSlot>)) {
			return Jboy.Json.ReadObject<List<ItemSlot>>(reader);
		} else if(fieldType == typeof(TimeStamp)) {
			return Jboy.Json.ReadObject<TimeStamp>(reader);
		} else if(fieldType == typeof(SkillBuild)) {
			return Jboy.Json.ReadObject<SkillBuild>(reader);
		//} else if(fieldType == typeof(WeaponBuild)) {
		//	return Jboy.Json.ReadObject<WeaponBuild>(reader);
		//} else if(fieldType == typeof(AttunementBuild)) {
		//	return Jboy.Json.ReadObject<AttunementBuild>(reader);
		} else if(fieldType == typeof(WeaponBuild[])) {
			return Jboy.Json.ReadObject<WeaponBuild[]>(reader);
		} else if(fieldType == typeof(AttunementBuild[])) {
			return Jboy.Json.ReadObject<AttunementBuild[]>(reader);
		} else if(fieldType == typeof(Guild)) {
			return GenericSerializer.ReadJSONClassInstance<Guild>(reader);
		} else if(fieldType == typeof(GuildMember)) {
			return GenericSerializer.ReadJSONClassInstance<GuildMember>(reader);
		} else if(fieldType == typeof(GuildMember[])) {
			return Jboy.Json.ReadObject<GuildMember[]>(reader);
		} else if(fieldType == typeof(List<string>)) {
			return Jboy.Json.ReadObject<List<string>>(reader);
		} else if(fieldType == typeof(Texture2D)) {
			return GenericSerializer.Texture2DJsonDeserializer(reader);
		} else {
			LogManager.General.LogError("Unknown field type for GenericSerializer.ReadJSONValue: " + fieldType);
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
				if(!field.IsStatic)
					field.SetValue(instance, GenericSerializer.ReadJSONValue(reader, field));
			} else {
				break;
			}
		}
		
		reader.ReadObjectEnd();
		
		return instance;
	}
	
	// Writer
	public static void ColorJsonSerializer(Jboy.JsonWriter writer, object instance) {
		Color col = (Color)instance;
		
		writer.WriteObjectStart();
		writer.WritePropertyName("r");
		writer.WriteNumber(col.r);
		writer.WritePropertyName("g");
		writer.WriteNumber(col.g);
		writer.WritePropertyName("b");
		writer.WriteNumber(col.b);
		writer.WritePropertyName("a");
		writer.WriteNumber(col.a);
		writer.WriteObjectEnd();
	}
	
	// Reader
	public static object ColorJsonDeserializer(Jboy.JsonReader reader) {
		reader.ReadObjectStart();
		reader.ReadPropertyName("r");
		var r = (float)reader.ReadNumber();
		reader.ReadPropertyName("g");
		var g = (float)reader.ReadNumber();
		reader.ReadPropertyName("b");
		var b = (float)reader.ReadNumber();
		reader.ReadPropertyName("a");
		var a = (float)reader.ReadNumber();
		reader.ReadObjectEnd();
		
		return new Color(r, g, b, a);
	}
	
	// Writer
	public static void Texture2DJsonSerializer(Jboy.JsonWriter writer, object instance) {
		writer.WriteNull();
		/*var tex = (Texture2D)instance;
		writer.WriteObjectStart();
		
		writer.WritePropertyName("width");
		writer.WriteNumber(tex.width);
		
		writer.WritePropertyName("height");
		writer.WriteNumber(tex.height);
		
		writer.WritePropertyName("data");
		Jboy.Json.WriteObject(tex.EncodeToPNG(), writer);
		
		writer.WriteObjectEnd();*/
	}
	
	// Reader
	public static object Texture2DJsonDeserializer(Jboy.JsonReader reader) {
		reader.ReadNull();
		return null; //new Texture2D(64, 64);
	}
}
