using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class GuildsServer : MonoBehaviour {
	private LobbyGameDB lobbyGameDB;
	private GuildsDB guildsDB;
	
	void Start () {
		lobbyGameDB = this.GetComponent<LobbyGameDB>();
		guildsDB = this.GetComponent<GuildsDB>();
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// Callbacks
	// --------------------------------------------------------------------------------
	
	// Once we have the guild ID list, send it to the player
	public static void OnReceiveGuildIdList(LobbyPlayer player) {
		//string guildListString = Jboy.Json.WriteObject(player.guildIdList);
		Lobby.RPC("ReceiveGuildIdList", player.peer, player.guildIdList.ToArray(), true);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void GuildIdListRequest(LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = LobbyServer.GetLobbyPlayer(info);
		
		if(lobbyPlayer.guildIdList == null) {
			StartCoroutine(guildsDB.GetGuildIdList(lobbyPlayer));
		} else {
			OnReceiveGuildIdList(lobbyPlayer);
		}
	}
	
	[RPC]
	IEnumerator GuildInfoRequest(string guildId, LobbyMessageInfo info) {
		// Get guild info from database
		if(!GameDB.guildIdToGuild.ContainsKey(guildId)) {
			yield return StartCoroutine(guildsDB.GetGuild(guildId));
		}
		
		// Send guild info to player
		if(GameDB.guildIdToGuild.ContainsKey(guildId)) {
			string guildInfoString = Jboy.Json.WriteObject(GameDB.guildIdToGuild[guildId]);
			Lobby.RPC("ReceiveGuildInfo", info.sender, guildId, guildInfoString);
		} else {
			Lobby.RPC("ReceiveGuildInfoError", info.sender, guildId);
		}
	}
	
	[RPC]
	IEnumerator GuildMembersRequest(string guildId, LobbyMessageInfo info) {
		// Get guild members from database
		if(!GameDB.guildIdToGuildMembers.ContainsKey(guildId)) {
			yield return StartCoroutine(guildsDB.GetGuildMembers(guildId));
		}
		
		// Send guild info to player
		if(GameDB.guildIdToGuildMembers.ContainsKey(guildId)) {
			var guildMembers = GameDB.guildIdToGuildMembers[guildId];
			
			// Member names
			foreach(var member in guildMembers) {
				if(GameDB.accountIdToName.ContainsKey(member.accountId)) {
					member.name = GameDB.accountIdToName[member.accountId];
				} else {
					yield return StartCoroutine(lobbyGameDB.GetPlayerName(member.accountId, data => {
						if(data != null) {
							member.name = data;
							GameDB.accountIdToName[member.accountId] = data;
						}
					}));
				}
			}
			
			Lobby.RPC("ReceiveGuildMembers", info.sender, guildId, guildMembers.ToArray(), true);
		} else {
			Lobby.RPC("ReceiveGuildMembersError", info.sender, guildId);
		}
	}
	
	[RPC]
	IEnumerator GuildInvitationRequest(string guildId, string playerName, LobbyMessageInfo info) {
		//LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		// TODO: Check if the player has guild invitation rights
		
		List<string> guildInvitations = null;
		string accountIdToInvite = null;
		
		// Get account ID
		yield return StartCoroutine(lobbyGameDB.GetAccountIdByPlayerName(playerName, data => {
			accountIdToInvite = data;
		}));
		
		if(accountIdToInvite == null) {
			Lobby.RPC("GuildInvitationError", info.sender, playerName);
			yield break;
		}
		
		// Get guild members
		if(!GameDB.guildIdToGuildMembers.ContainsKey(guildId)) {
			yield return StartCoroutine(guildsDB.GetGuildMembers(guildId));
		}
		
		// Already a member?
		var guildMembers = GameDB.guildIdToGuildMembers[guildId];
		if(guildMembers.Find(m => m.accountId == accountIdToInvite) != null) {
			Lobby.RPC("GuildInvitationAlreadyMember", info.sender, playerName);
			yield break;
		}
		
		// Get guild invitations
		if(LobbyPlayer.accountIdToLobbyPlayer.ContainsKey(accountIdToInvite)) {
			guildInvitations = LobbyPlayer.accountIdToLobbyPlayer[accountIdToInvite].guildInvitations;
		}
		
		if(guildInvitations == null) {
			yield return StartCoroutine(guildsDB.GetGuildInvitations(accountIdToInvite, data => {
				if(data == null) {
					guildInvitations = new List<string>();
				} else {
					guildInvitations = data;
				}
			}));
		}
		
		if(guildInvitations == null) {
			Lobby.RPC("GuildInvitationError", info.sender, playerName);
			yield break;
		}
		
		// Guild invitation already sent?
		if(guildInvitations.Contains(guildId)) {
			Lobby.RPC("GuildInvitationAlreadySent", info.sender, playerName);
			yield break;
		}
		
		// Add guild to invitation list
		guildInvitations.Add(guildId);
		
		// Set guild invitations
		yield return StartCoroutine(guildsDB.SetGuildInvitations(accountIdToInvite, guildInvitations, data => {
			if(data == null) {
				Lobby.RPC("GuildInvitationError", info.sender, playerName);
			} else {
				if(LobbyPlayer.accountIdToLobbyPlayer.ContainsKey(accountIdToInvite)) {
					LobbyPlayer.accountIdToLobbyPlayer[accountIdToInvite].guildInvitations = data;
				}
				
				Lobby.RPC("GuildInvitationSuccess", info.sender, playerName);
			}
		}));
	}
	
	[RPC]
	IEnumerator GuildInvitationsListRequest(LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = LobbyServer.GetLobbyPlayer(info);
		
		// Get guild invitations
		if(lobbyPlayer.guildInvitations == null) {
			yield return StartCoroutine(guildsDB.GetGuildInvitations(lobbyPlayer.account.id.value, data => {
				if(data == null) {
					lobbyPlayer.guildInvitations = new List<string>();
				} else {
					lobbyPlayer.guildInvitations = data;
				}
			}));
		}
		
		Lobby.RPC("ReceiveGuildInvitationsList", lobbyPlayer.peer, lobbyPlayer.guildInvitations.ToArray(), true);
	}
	
	[RPC]
	IEnumerator GuildInvitationResponse(string guildId, bool accepted, LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = LobbyServer.GetLobbyPlayer(info);
		
		// Get guild invitations
		if(lobbyPlayer.guildInvitations == null) {
			yield return StartCoroutine(guildsDB.GetGuildInvitations(lobbyPlayer.account.id.value, data => {
				if(data == null) {
					lobbyPlayer.guildInvitations = new List<string>();
				} else {
					lobbyPlayer.guildInvitations = data;
				}
			}));
		}
		
		if(lobbyPlayer.guildInvitations == null) {
			Lobby.RPC("GuildInvitationResponseError", info.sender, guildId);
			yield break;
		}
		
		// Were you invited?
		if(!lobbyPlayer.guildInvitations.Contains(guildId)) {
			Lobby.RPC("GuildInvitationResponseError", info.sender, guildId);
			yield break;
		}
		
		// Did the player accept the invitation?
		if(accepted) {
			// Get guild members from database
			if(!GameDB.guildIdToGuildMembers.ContainsKey(guildId)) {
				yield return StartCoroutine(guildsDB.GetGuildMembers(guildId));
			}
			
			var guildMembers = GameDB.guildIdToGuildMembers[guildId];
			guildMembers.Add(new GuildMember(lobbyPlayer.account.id.value, (byte)GuildMember.Rank.Default));
			
			// Set guild members
			yield return StartCoroutine(guildsDB.SetGuildMembers(guildId, guildMembers));
			
			// Get guild ID list
			if(lobbyPlayer.guildIdList == null) {
				yield return StartCoroutine(guildsDB.GetGuildIdList(lobbyPlayer));
			}
			
			// Add to guild ID list
			lobbyPlayer.guildIdList.Add(guildId);
			
			// Set guild ID list
			yield return StartCoroutine(guildsDB.SetGuildIdList(lobbyPlayer));
		}
		
		// Remove guild from invitation list
		lobbyPlayer.guildInvitations.Remove(guildId);
		
		// Set guild invitations
		yield return StartCoroutine(guildsDB.SetGuildInvitations(lobbyPlayer.account.id.value, lobbyPlayer.guildInvitations, data => {
			if(data == null) {
				Lobby.RPC("GuildInvitationResponseError", info.sender, guildId);
			} else {
				lobbyPlayer.guildInvitations = data;
				Lobby.RPC("GuildInvitationResponseSuccess", info.sender, guildId, accepted);
			}
		}));
	}
	
	[RPC]
	IEnumerator GuildCreationRequest(string name, string tag, LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = LobbyServer.GetLobbyPlayer(info);
		
		if(name.Length > GameDB.maxGuildNameLength) {
			Lobby.RPC("GuildNameLengthError", info.sender);
			yield break;
		}
		
		if(tag.Length > GameDB.maxGuildTagLength) {
			Lobby.RPC("GuildTagLengthError", info.sender);
			yield break;
		}
		
		// Store new guild in database
		yield return StartCoroutine(guildsDB.PutGuild(new Guild(name, tag, lobbyPlayer.account.id.value), lobbyPlayer));
		
		// Store new guild membership in database
		yield return StartCoroutine(guildsDB.SetGuildIdList(lobbyPlayer));
		
		// Let the player know that it worked
		Lobby.RPC("GuildCreationSuccess", info.sender);
		OnReceiveGuildIdList(lobbyPlayer);
	}
}
