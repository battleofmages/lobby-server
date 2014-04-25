using System.Collections;
using System.Collections.Generic;

public static class GuildsDB {
	// --------------------------------------------------------------------------------
	// Guilds
	// --------------------------------------------------------------------------------
	
	// Put guild
	public static IEnumerator PutGuild(Guild guild, GameDB.PutActionOnResult<Guild> func) {
		yield return GameDB.instance.StartCoroutine(GameDB.Put<Guild>(
			"Guilds",
			guild,
			func
		));
	}
	
	// Set guild
	public static IEnumerator SetGuild(string guildId, Guild guild) {
		yield return GameDB.instance.StartCoroutine(GameDB.Set<Guild>(
			"Guilds",
			guildId,
			guild,
			data => {
				// ...
			}
		));
	}
	
	// Get guild
	public static IEnumerator GetGuild(string guildId) {
		yield return GameDB.instance.StartCoroutine(GameDB.Get<Guild>(
			"Guilds",
			guildId,
			data => {
				if(data == null) {
					// ...
				} else {
					GameDB.guildIdToGuild[guildId] = data;
				}
			}
		));
	}
	
	// Get guild
	public static IEnumerator GetGuild(string guildId, GameDB.ActionOnResult<Guild> func) {
		yield return GameDB.instance.StartCoroutine(GameDB.Get<Guild>(
			"Guilds",
			guildId,
			func
		));
	}
	
	// Remove guild members
	public static IEnumerator RemoveGuild(string guildId) {
		yield return GameDB.instance.StartCoroutine(GameDB.Remove(
			"Guilds",
			guildId,
			success => {
				if(success) {
					GameDB.guildIdToGuild.Remove(guildId);
				}
			}
		));
	}
	
	// --------------------------------------------------------------------------------
	// AccountToGuilds
	// --------------------------------------------------------------------------------
	
	// Get guild list
	public static IEnumerator GetGuildList(string accountId, GameDB.ActionOnResult<GuildList> func) {
		yield return GameDB.instance.StartCoroutine(GameDB.Get<GuildList>(
			"AccountToGuilds",
			accountId,
			func
		));
	}
	
	// Set guild list
	public static IEnumerator SetGuildList(string accountId, GuildList guildIdList, GameDB.ActionOnResult<GuildList> func = null) {
		yield return GameDB.instance.StartCoroutine(GameDB.Set<GuildList>(
			"AccountToGuilds",
			accountId,
			guildIdList,
			func
		));
	}
	
	// --------------------------------------------------------------------------------
	// GuildToMembers
	// --------------------------------------------------------------------------------
	
	// Set guild members
	public static IEnumerator SetGuildMembers(string guildId, List<GuildMember> members) {
		yield return GameDB.instance.StartCoroutine(GameDB.Set<List<GuildMember>>(
			"GuildToMembers",
			guildId,
			members,
			data => {
				// ...
			}
		));
	}
	
	// Get guild members
	public static IEnumerator GetGuildMembers(string guildId) {
		yield return GameDB.instance.StartCoroutine(GameDB.Get<List<GuildMember>>(
			"GuildToMembers",
			guildId,
			data => {
				if(data == null) {
					// ...
				} else {
					GameDB.guildIdToGuildMembers[guildId] = data;
				}
			}
		));
	}
	
	// Remove guild members
	public static IEnumerator RemoveGuildMembers(string guildId) {
		yield return GameDB.instance.StartCoroutine(GameDB.Remove(
			"GuildToMembers",
			guildId,
			success => {
				if(success) {
					GameDB.guildIdToGuildMembers.Remove(guildId);
				}
			}
		));
	}
	
	// --------------------------------------------------------------------------------
	// AccountToGuildInvitations
	// --------------------------------------------------------------------------------
	
	// Set guild invitations
	public static IEnumerator SetGuildInvitations(string accountId, List<string> gInvitations, GameDB.ActionOnResult<List<string>> func) {
		yield return GameDB.instance.StartCoroutine(GameDB.Set<List<string>>(
			"AccountToGuildInvitations",
			accountId,
			gInvitations,
			func
		));
	}
	
	// Get guild invitations
	public static IEnumerator GetGuildInvitations(string accountId, GameDB.ActionOnResult<List<string>> func) {
		yield return GameDB.instance.StartCoroutine(GameDB.Get<List<string>>(
			"AccountToGuildInvitations",
			accountId,
			func
		));
	}
	
	// --------------------------------------------------------------------------------
	// MapReduce
	// --------------------------------------------------------------------------------
	
	// Get guild ID by guild name
	public static IEnumerator GetGuildIdByGuildName(string guildName, GameDB.ActionOnResult<string> func) {
		yield return GameDB.instance.StartCoroutine(GameDB.MapReduce<KeyValue<string>>(
			"Guilds",
			GameDB.GetSearchMapFunction("name"),
			GameDB.GetSearchReduceFunction(),
			guildName,
			data => {
				if(data != null && data.Length == 1) {
					func(data[0].key);
				} else {
					func(default(string));
				}
			}
		));
	}
}
