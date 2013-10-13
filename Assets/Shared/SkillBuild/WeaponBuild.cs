﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class WeaponBuild {
	public int weaponId;
	public AttunementBuild[] attunements;
	
	public WeaponBuild() {
		
	}
	
	// BitStream Writer
	public static void WriteToBitStream(uLink.BitStream stream, object val, params object[] args) {
		var obj = (WeaponBuild)val;
		
		stream.WriteByte((byte)obj.weaponId);
		stream.Write<AttunementBuild[]>(obj.attunements);
	}
	
	// BitStream Reader
	public static object ReadFromBitStream(uLink.BitStream stream, params object[] args) {
		var obj = new WeaponBuild();
		
		obj.weaponId = (int)stream.ReadByte();
		obj.attunements = stream.Read<AttunementBuild[]>();
		
		return obj;
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<WeaponBuild>(writer, (WeaponBuild)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<WeaponBuild>(reader);
	}
}
