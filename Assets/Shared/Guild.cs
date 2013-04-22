using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Guild {
	public string name;
	public string tag;
	public string introduction;
	public string messageOfTheDay;
	public TimeStamp creationDate;
	public string founderAccountId;
	public Texture2D icon;
	
	public Guild() {
		name = "";
		tag = "";
		introduction = "";
		messageOfTheDay = "";
		creationDate = new TimeStamp();
		icon = null; //new Texture2D(64, 64);
	}
	
	public Guild(string guildName, string guildTag, string nFounderAccountId) {
		name = guildName;
		tag = guildTag;
		introduction = "";
		messageOfTheDay = "";
		creationDate = new TimeStamp();
		founderAccountId = nFounderAccountId;
		icon = null;
	}
	
	public override string ToString() {
		return name + " [" + tag + "]";
	}
	
	// Can the account invite persons?
	public static bool CanInvite(string guildId, string accountId) {
		try {
			return GameDB.guildIdToGuildMembers[guildId].Find(o => o.accountId == accountId).rank == (byte)GuildMember.Rank.Leader;
		} catch {
			return false;
		}
	}
	
	// Can the account kick persons?
	public static bool CanKick(string guildId, string accountId) {
		try {
			return GameDB.guildIdToGuildMembers[guildId].Find(o => o.accountId == accountId).rank == (byte)GuildMember.Rank.Leader;
		} catch {
			return false;
		}
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
