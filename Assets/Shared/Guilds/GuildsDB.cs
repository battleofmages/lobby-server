using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class GuildsDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// Guilds
	// --------------------------------------------------------------------------------
	
	// Put guild
	public IEnumerator PutGuild(Guild guild, GameDB.PutActionOnResult<Guild> func) {
		yield return StartCoroutine(GameDB.Put<Guild>(
			"Guilds",
			guild,
			func
		));
	}
	
	// Set guild
	public IEnumerator SetGuild(string guildId, Guild guild) {
		yield return StartCoroutine(GameDB.Set<Guild>(
			"Guilds",
			guildId,
			guild,
			data => {
				// ...
			}
		));
	}
	
	// Get guild
	public IEnumerator GetGuild(string guildId) {
		yield return StartCoroutine(GameDB.Get<Guild>(
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
	public IEnumerator GetGuild(string guildId, GameDB.ActionOnResult<Guild> func) {
		yield return StartCoroutine(GameDB.Get<Guild>(
			"Guilds",
			guildId,
			func
		));
	}
	
	// Remove guild members
	public IEnumerator RemoveGuild(string guildId) {
		yield return StartCoroutine(GameDB.Remove(
			"Guilds",
			guildId,
			success => {
				if(success) {
					if(GameDB.guildIdToGuild.ContainsKey(guildId))
						GameDB.guildIdToGuild.Remove(guildId);
				}
			}
		));
	}
	
	// --------------------------------------------------------------------------------
	// AccountToGuilds
	// --------------------------------------------------------------------------------
	
	// Get guild list
	public IEnumerator GetGuildList(string accountId, GameDB.ActionOnResult<GuildList> func) {
		yield return StartCoroutine(GameDB.Get<GuildList>(
			"AccountToGuilds",
			accountId,
			func
		));
	}
	
	// Set guild list
	public IEnumerator SetGuildList(string accountId, GuildList guildIdList, GameDB.ActionOnResult<GuildList> func = null) {
		yield return StartCoroutine(GameDB.Set<GuildList>(
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
	public IEnumerator SetGuildMembers(string guildId, List<GuildMember> members) {
		yield return StartCoroutine(GameDB.Set<List<GuildMember>>(
			"GuildToMembers",
			guildId,
			members,
			data => {
				// ...
			}
		));
	}
	
	// Get guild members
	public IEnumerator GetGuildMembers(string guildId) {
		yield return StartCoroutine(GameDB.Get<List<GuildMember>>(
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
	public IEnumerator RemoveGuildMembers(string guildId) {
		yield return StartCoroutine(GameDB.Remove(
			"GuildToMembers",
			guildId,
			success => {
				if(success) {
					if(GameDB.guildIdToGuildMembers.ContainsKey(guildId))
						GameDB.guildIdToGuildMembers.Remove(guildId);
				}
			}
		));
	}
	
	// --------------------------------------------------------------------------------
	// AccountToGuildInvitations
	// --------------------------------------------------------------------------------
	
	// Set guild invitations
	public IEnumerator SetGuildInvitations(string accountId, List<string> gInvitations, GameDB.ActionOnResult<List<string>> func) {
		yield return StartCoroutine(GameDB.Set<List<string>>(
			"AccountToGuildInvitations",
			accountId,
			gInvitations,
			func
		));
	}
	
	// Get guild invitations
	public IEnumerator GetGuildInvitations(string accountId, GameDB.ActionOnResult<List<string>> func) {
		yield return StartCoroutine(GameDB.Get<List<string>>(
			"AccountToGuildInvitations",
			accountId,
			func
		));
	}
	
	// --------------------------------------------------------------------------------
	// MapReduce
	// --------------------------------------------------------------------------------
	
	// Get guild ID by guild name
	public IEnumerator GetGuildIdByGuildName(string guildName, GameDB.ActionOnResult<string> func) {
		yield return StartCoroutine(GameDB.MapReduce<KeyToValueEntry>(
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
