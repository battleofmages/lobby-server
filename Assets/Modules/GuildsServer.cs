using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	
	// Sends the member list after a change
	void SendGuildMemberList(string guildId, List<GuildMember> guildMembers) {
		foreach(var member in guildMembers) {
			if(LobbyPlayer.AccountIsOnline(member.accountId)) {
				var memberAsLobbyPlayer = LobbyPlayer.accountIdToLobbyPlayer[member.accountId];
				Lobby.RPC("ReceiveGuildMembers", memberAsLobbyPlayer.peer, guildId, guildMembers.ToArray(), true);
			}
		}
	}
	
	// --------------------------------------------------------------------------------
	// Callbacks
	// --------------------------------------------------------------------------------
	
	// Once we have the guild ID list, send it to the player
	public static void SendGuildList(LobbyPlayer player) {
		string guildListString = Jboy.Json.WriteObject(player.guildList);
		Lobby.RPC("ReceiveGuildList", player.peer, guildListString);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
#region Client list requests
	[RPC]
	void GuildListRequest(LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = LobbyServer.GetLobbyPlayer(info);
		
		if(lobbyPlayer.guildList == null) {
			StartCoroutine(guildsDB.GetGuildList(lobbyPlayer.accountId, data => {
				if(data == null) {
					lobbyPlayer.guildList = new GuildList();
				} else {
					lobbyPlayer.guildList = data;
				}
				
				GuildsServer.SendGuildList(lobbyPlayer);
			}));
		} else {
			GuildsServer.SendGuildList(lobbyPlayer);
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
#endregion
	
#region Guild invitations
	[RPC]
	IEnumerator GuildInvitationRequest(string guildId, string playerName, LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = LobbyServer.GetLobbyPlayer(info);
		
		// Check if the player has guild invitation rights
		if(!Guild.CanInvite(guildId, lobbyPlayer.accountId)) {
			Lobby.RPC("GuildInvitationError", info.sender, playerName);
			yield break;
		}
		
		List<string> guildInvitations = null;
		string accountIdToInvite = null;
		
		// Get account ID
		yield return StartCoroutine(lobbyGameDB.GetAccountIdByPlayerName(playerName, data => {
			accountIdToInvite = data;
		}));
		
		if(accountIdToInvite == null) {
			Lobby.RPC("GuildInvitationPlayerDoesntExistError", info.sender, playerName);
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
				// Notify player if he is online
				if(LobbyPlayer.accountIdToLobbyPlayer.ContainsKey(accountIdToInvite)) {
					var invitedPlayer = LobbyPlayer.accountIdToLobbyPlayer[accountIdToInvite];
					invitedPlayer.guildInvitations = data;
					Lobby.RPC("ReceiveGuildInvitationsList", invitedPlayer.peer, invitedPlayer.guildInvitations.ToArray(), true);
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
			yield return StartCoroutine(guildsDB.GetGuildInvitations(lobbyPlayer.accountId, data => {
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
			yield return StartCoroutine(guildsDB.GetGuildInvitations(lobbyPlayer.accountId, data => {
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
			guildMembers.Add(new GuildMember(lobbyPlayer.accountId, lobbyPlayer.name, (byte)GuildMember.Rank.Default));
			
			// Set guild members
			yield return StartCoroutine(guildsDB.SetGuildMembers(guildId, guildMembers));
			
			// Get guild ID list
			if(lobbyPlayer.guildList == null) {
				yield return StartCoroutine(guildsDB.GetGuildList(lobbyPlayer.accountId, data => {
					if(data == null) {
						lobbyPlayer.guildList = new GuildList();
					} else {
						lobbyPlayer.guildList = data;
					}
				}));
			}
			
			// Add to guild ID list
			lobbyPlayer.guildList.Add(guildId);
			
			// Set guild ID list
			yield return StartCoroutine(guildsDB.SetGuildList(lobbyPlayer.accountId, lobbyPlayer.guildList));
			
			// Notify all guild members
			SendGuildMemberList(guildId, guildMembers);
		}
		
		// Remove guild from invitation list
		lobbyPlayer.guildInvitations.Remove(guildId);
		
		// Set guild invitations
		yield return StartCoroutine(guildsDB.SetGuildInvitations(lobbyPlayer.accountId, lobbyPlayer.guildInvitations, data => {
			if(data == null) {
				Lobby.RPC("GuildInvitationResponseError", info.sender, guildId);
			} else {
				lobbyPlayer.guildInvitations = data;
				Lobby.RPC("GuildInvitationResponseSuccess", info.sender, guildId, accepted);
				SendGuildList(lobbyPlayer);
			}
		}));
	}
#endregion
	
	[RPC]
	IEnumerator GuildRepresentRequest(string guildId, bool represent, LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = LobbyServer.GetLobbyPlayer(info);
		string accountId = lobbyPlayer.accountId;
		
		// Get guild members from database
		if(!GameDB.guildIdToGuildMembers.ContainsKey(guildId)) {
			yield return StartCoroutine(guildsDB.GetGuildMembers(guildId));
		}
		
		var guildMembers = GameDB.guildIdToGuildMembers[guildId];
		var index = guildMembers.FindIndex(o => o.accountId == accountId);
		
		if(index == -1) {
			Lobby.RPC("GuildRepresentError", info.sender, guildId, represent);
			yield break;
		}
		
		// Get guild ID list
		if(lobbyPlayer.guildList == null) {
			yield return StartCoroutine(guildsDB.GetGuildList(accountId, data => {
				if(data == null) {
					lobbyPlayer.guildList = new GuildList();
				} else {
					lobbyPlayer.guildList = data;
				}
			}));
		}
		
		// Set/unset main guild
		if(represent) {
			// Start representing
			lobbyPlayer.guildList.mainGuildId = guildId;
		} else {
			// Stop representing
			if(lobbyPlayer.guildList.mainGuildId == guildId) {
				lobbyPlayer.guildList.mainGuildId = "";
			}
		}
		
		// Set guild ID list
		yield return StartCoroutine(guildsDB.SetGuildList(accountId, lobbyPlayer.guildList));
		
		Lobby.RPC("GuildRepresentSuccess", info.sender, guildId, represent);
	}
	
	[RPC]
	IEnumerator GuildKickRequest(string guildId, string accountId, LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = LobbyServer.GetLobbyPlayer(info);
		
		// Does the player have rights to kick
		if(!Guild.CanKick(guildId, lobbyPlayer.accountId)) {
			Lobby.RPC("GuildKickError", info.sender, guildId, accountId);
			yield break;
		}
		
		// Get guild members from database
		if(!GameDB.guildIdToGuildMembers.ContainsKey(guildId)) {
			yield return StartCoroutine(guildsDB.GetGuildMembers(guildId));
		}
		
		var guildMembers = GameDB.guildIdToGuildMembers[guildId];
		var index = guildMembers.FindIndex(o => o.accountId == accountId);
		
		if(index == -1) {
			Lobby.RPC("GuildKickError", info.sender, guildId, accountId);
			yield break;
		}
		
		guildMembers.RemoveAt(index);
		
		// Set guild members
		yield return StartCoroutine(guildsDB.SetGuildMembers(guildId, guildMembers));
		
		// Player online?
		if(LobbyPlayer.accountIdToLobbyPlayer.ContainsKey(accountId)) {
			var playerKicked = LobbyPlayer.accountIdToLobbyPlayer[accountId];
			
			// Get guild ID list
			if(playerKicked.guildList == null) {
				yield return StartCoroutine(guildsDB.GetGuildList(accountId, data => {
					if(data == null) {
						playerKicked.guildList = new GuildList();
					} else {
						playerKicked.guildList = data;
					}
				}));
			}
			
			// Remove guild ID from the kicked player's guild ID list
			playerKicked.guildList.Remove(guildId);
			
			// Set guild ID list
			yield return StartCoroutine(guildsDB.SetGuildList(accountId, playerKicked.guildList));
			
			// Send the kicked player the new guild ID list
			SendGuildList(playerKicked);
		// Player offline
		} else {
			GuildList guildList = null;
			
			// Get guild ID list
			yield return StartCoroutine(guildsDB.GetGuildList(accountId, data => {
				guildList = data;
			}));
			
			if(guildList == null) {
				Lobby.RPC("GuildKickError", info.sender, guildId, accountId);
				yield break;
			}
			
			// Remove guild ID from the kicked player's guild ID list
			guildList.Remove(guildId);
			
			// Set guild ID list
			yield return StartCoroutine(guildsDB.SetGuildList(accountId, guildList));
		}
		
		// Notify all guild members
		SendGuildMemberList(guildId, guildMembers);
	}
	
	[RPC]
	IEnumerator GuildCreationRequest(string name, string tag, LobbyMessageInfo info) {
		LobbyPlayer founder = LobbyServer.GetLobbyPlayer(info);
		
		// Protection against modified RPC packets
		if(!Validator.guildName.IsMatch(name)) {
			yield break;
		}
		
		if(!Validator.guildTag.IsMatch(tag)) {
			yield break;
		}
		
		// Check if guild name has already been registered
		bool guildNameExists = false;
		
		yield return StartCoroutine(guildsDB.GetGuildIdByGuildName(name, data => {
			if(data != null) {
				guildNameExists = true;
			}
		}));
		
		if(guildNameExists) {
			Lobby.RPC("GuildNameAlreadyExists", info.sender);
			yield break;
		}
		
		// Store new guild in database
		string guildId = null;
		var guild = new Guild(name, tag, founder.accountId);
		
		yield return StartCoroutine(guildsDB.PutGuild(
			guild,
			(key, data) => {
				if(key != null) {
					if(founder.guildList == null)
						founder.guildList = new GuildList();
					
					guildId = key;
				}
			}
		));
		
		if(guildId == null) {
			Lobby.RPC("GuildCreationError", info.sender);
			yield break;
		}
		
		// Founder joins the guild automatically
		var memberList = new List<GuildMember>(); //GameDB.guildIdToGuildMembers[guildId];
		memberList.Add(new GuildMember(founder.accountId, (byte)GuildMember.Rank.Leader));
		yield return StartCoroutine(guildsDB.SetGuildMembers(guildId, memberList));
		
		founder.guildList.Add(guildId);
		
		// Store new guild membership in database
		yield return StartCoroutine(guildsDB.SetGuildList(founder.accountId, founder.guildList));
		
		// Let the player know that it worked
		Lobby.RPC("GuildCreationSuccess", info.sender);
		
		// Send him the new guild ID list
		SendGuildList(founder);
		
		XDebug.Log("Guild " + guild + " has been created.");
	}
}
