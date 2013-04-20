using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class GuildsDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// Guilds
	// --------------------------------------------------------------------------------
	
	// Put guild
	public IEnumerator PutGuild(Guild guild, LobbyPlayer founder) {
		string guildId = "";
		
		yield return StartCoroutine(GameDB.Put<Guild>(
			"Guilds",
			guild,
			(key, data) => {
				if(founder.guildIdList == null)
					founder.guildIdList = new List<string>();
				
				guildId = key;
			}
		));
		
		// Founder joins the guild automatically
		var memberList = new List<GuildMember>(); //GameDB.guildIdToGuildMembers[guildId];
		memberList.Add(new GuildMember(founder.account.id.value, (byte)GuildMember.Rank.Leader));
		yield return StartCoroutine(SetGuildMembers(guildId, memberList));
		
		founder.guildIdList.Add(guildId);
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
	
	// --------------------------------------------------------------------------------
	// AccountToGuilds
	// --------------------------------------------------------------------------------
	
	// Get guild ID list
	public IEnumerator GetGuildIdList(LobbyPlayer lobbyPlayer) {
		yield return StartCoroutine(GameDB.Get<List<string>>(
			"AccountToGuilds",
			lobbyPlayer.account.id.value,
			data => {
				if(data == null) {
					lobbyPlayer.guildIdList = new List<string>();
				} else {
					lobbyPlayer.guildIdList = data;
				}
			}
		));
		
		//XDebug.Log("Received guild ID list: " + lobbyPlayer.guildIdList);
		GuildsServer.OnReceiveGuildIdList(lobbyPlayer);
	}
	
	// Set guild ID list
	public IEnumerator SetGuildIdList(LobbyPlayer lobbyPlayer) {
		yield return StartCoroutine(GameDB.Set<List<string>>(
			"AccountToGuilds",
			lobbyPlayer.account.id.value,
			lobbyPlayer.guildIdList,
			data => {
				// ...
			}
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
}
