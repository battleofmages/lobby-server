using UnityEngine;
using System.Collections;

[System.Serializable]
public class AttunementBuild {
	public int attunementId;
	public int[] skills;
	
	public AttunementBuild() {
		
	}
	
	// BitStream Writer
	public static void WriteToBitStream(uLink.BitStream stream, object val, params object[] args) {
		var obj = (AttunementBuild)val;
		
		stream.WriteByte((byte)obj.attunementId);
		stream.Write<int[]>(obj.skills);
	}
	
	// BitStream Reader
	public static object ReadFromBitStream(uLink.BitStream stream, params object[] args) {
		var obj = new AttunementBuild();
		
		obj.attunementId = (int)stream.ReadByte();
		obj.skills = stream.Read<int[]>();
		
		return obj;
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<AttunementBuild>(writer, (AttunementBuild)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<AttunementBuild>(reader);
	}
}
