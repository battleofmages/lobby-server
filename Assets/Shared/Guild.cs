using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class GuildMember {
	public string accountId;
	public byte rank;
	public TimeStamp joinDate;
	
	[System.NonSerialized]
	public string name;
	
	public enum Rank {
		Leader,
		Default
	}
	
	public GuildMember() {
		accountId = "";
		name = "";
		rank = 0;
		joinDate = new TimeStamp();
	}
	
	public GuildMember(string nAccountId, byte nRank) {
		accountId = nAccountId;
		name = "";
		rank = nRank;
		joinDate = new TimeStamp();
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<GuildMember>(writer, (GuildMember)instance, null, new HashSet<string>(){
			"name"
		});
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<GuildMember>(reader);
	}
	
	// BitStream Writer
	public static void WriteToBitStream(uLink.BitStream stream, object val, params object[] args) {
		GuildMember obj = (GuildMember)val;
		
		stream.WriteString(obj.name);
		stream.WriteByte(obj.rank);
		
		// TODO: Transfer joinDate
	}
	
	// BitStream Reader
	public static object ReadFromBitStream(uLink.BitStream stream, params object[] args) {
		GuildMember obj = new GuildMember();
		
		obj.name = stream.ReadString();
		obj.rank = stream.ReadByte();
		
		return obj;
	}
}

[System.Serializable]
public class Guild {
	public string name;
	public string tag;
	public double level;
	public string introduction;
	public TimeStamp creationDate;
	public string founderAccountId;
	public Texture2D icon;
	
	public Guild() {
		name = "";
		tag = "";
		level = 1d;
		creationDate = new TimeStamp();
		icon = null; //new Texture2D(64, 64);
	}
	
	public Guild(string guildName, string guildTag, string nFounderAccountId) {
		name = guildName;
		tag = guildTag;
		level = 1d;
		creationDate = new TimeStamp();
		founderAccountId = nFounderAccountId;
		icon = null;
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<Guild>(writer, (Guild)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<Guild>(reader);
	}
}
