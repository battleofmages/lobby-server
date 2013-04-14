using UnityEngine;
using uLobby;
using uGameDB;
using uZone;
using System.Collections;
using System.Collections.Generic;

/*
// Bucket: AccountToName

 - Name : string

// Bucket: AccountToRanking

 - Ranking : int

// Bucket: AccountToStats

 - Level : ushort
 - Kills : int
 - Deaths : int
 - Damage : int64
 - CC : int64
 - Wins : int
 - Losses : int

(// Bucket: NameToAccount

 - AccountID : string)
*/

public class LobbyServer : MonoBehaviour {
	public static string gameName = "bomWithFlags";
	public static LobbyChatChannel globalChannel = new LobbyChatChannel("Global");
	public static LobbyGameDB lobbyGameDB;
	
	public int maxConnections = 1024;
	public int listenPort = 1310;
	public string databaseHost = "battle-of-mages.com";
	public int databasePort = 8087;
	public string uZoneHost = "127.0.0.1";
	public int uZonePort = 12345;
	
	private int serverVersionNumber;
	
	private LobbyQueue[] queue;
	
	void Awake() {
		// Limit frame rate
		//Application.targetFrameRate = 20;
		uLinkTargetFrameRateFix.SetTargetFrameRate(20);
		
		// Initialize uZone
		uZone.InstanceManager.Initialise();
		uZone.InstanceManager.Connect(uZoneHost, uZonePort);
		uZone.InstanceManager.AddListener(this.gameObject);
		
		// Create queues
		queue = new LobbyQueue[5];
		for(int i = 0; i < queue.Length; i++) {
			queue[i] = new LobbyQueue();
			queue[i].unitsNeededForGameStart = (i + 1) * 2;
		}
		
		// Register codecs for serialization
		GameDB.InitCodecs();
	}
	
	// Use this for initialization
	void Start () {
		// Get lobby game DB component
		lobbyGameDB = this.GetComponent<LobbyGameDB>();
		
		// Version number
		serverVersionNumber = this.GetComponent<Version>().versionNumber;
		
		// Event handlers
		Lobby.OnLobbyInitialized += OnLobbyInitialized;
		Lobby.OnSecurityInitialized += OnSecurityInitialized;
		
		// Make this class listen to Lobby events
		Lobby.AddListener(this);
		
		// Initialize the lobby
		XDebug.Log("Initializing lobby on port " + listenPort + " with a maximum of " + maxConnections + " players.");
		Lobby.InitializeLobby(maxConnections, listenPort, databaseHost, databasePort);
	}
	
	void RemovePlayer(LobbyPlayer player) {
		// Remove the player from the queue he was in
		if(player.queue != null)
			player.queue.RemovePlayer(player);
		
		// Remove the reference from the dictionary
		LobbyPlayer.accountIdToLobbyPlayer.Remove(player.account.id.value);
		
		// Remove the player from the global list
		LobbyPlayer.list.Remove(player);
		
		// Remove the player from all chat channels
		foreach(var channel in new List<LobbyChatChannel>(player.channels)) {
			channel.RemovePlayer(player);
		}
		
		// Log it
		XDebug.Log("Player '" + player.name + "' from account '" + player.account.name + "' logged out.");
	}
	
	// We send players information about the queues each second
	void SendQueueStats() {
		var offlinePlayers = new List<LobbyPlayer>();
		int playerCount = LobbyPlayer.list.Count;
		
		// TODO: Players need to request queue stats (to not send data to AFK players)
		foreach(LobbyPlayer player in LobbyPlayer.list) {
			if(player.inMatch)
				continue;
			
			// If for some reason the player is still in the list after being disconnected
			// add him to the offlinePlayers list and remove him later.
			if(!Lobby.IsPeerConnected(player.peer)) {
				offlinePlayers.Add(player);
				continue;
			}
			
			// Send information to the player about the queues
			try {
				Lobby.RPC("QueueStats",
					player.peer,
					playerCount,
					queue[0].playerCount,
					queue[1].playerCount,
					queue[2].playerCount,
					queue[3].playerCount,
					queue[4].playerCount
				);
			} catch {
				XDebug.Log("Couldn't send queue data to player '" + player.name + "' from account '" + player.account.name + "'");
			}
		}
		
		// Clear offline players
		foreach(LobbyPlayer player in offlinePlayers) {
			RemovePlayer(player);
		}
	}
	
	// Gets the lobby player by the supplied message info
	LobbyPlayer GetLobbyPlayer(LobbyMessageInfo info) {
		Account account = AccountManager.Master.GetLoggedInAccount(info.sender);
		return LobbyPlayer.accountIdToLobbyPlayer[account.id.value];
	}
	
	// --------------------------------------------------------------------------------
	// Callbacks
	// --------------------------------------------------------------------------------
	
	// Lobby initialized
	void OnLobbyInitialized() {
		XDebug.Log("Successfully initialized lobby.");
		
		// Public key
		Lobby.privateKey = new uLobby.PrivateKey(
@"<RSAKeyValue>
<Modulus>td076m4fBadO7bRuEkoOaeaQT+TTqMVEWOEXbUBRXZwf1uR0KE8A/BbOWNripW1eZinvsC+skgVT/G8mrhYTWVl0TrUuyOV6rpmgl5PnoeLneQDEfrGwFUR4k4ijDcSlNpUnfL3bBbUaI5XjPtXD+2Za2dRXT3GDMrePM/QO8xE=</Modulus>
<Exponent>EQ==</Exponent>
<P>yKHtauTiTeBpUlHDHIya+3p0/YSWrUTJGgsx8tPW7hT4mq9DySSvGd1SzWLBdZ1BWpIA0l2jmK3ptLJjGIc3pw==</P>
<Q>6A1hp1ZZ/0o7dULdFXvRJvRCTX5rQaUFYWFn7uRvxneMSKA/6SNLzxr91N2tILQx4vbXSOoO0w7DyS64qU3Whw==</Q>
<DP>OwJzAVJgrX49GDYqU7DiSfbXHWM7YCNKNNYdv+Pz66vQpfdQLBnZJbmQ0v7tmxAiR9CW1HXk0o2A+OksNGQBTw==</DP>
<DQ>sXOlB35E0kfTHW9dxSJyw299/wZSBQW40f8xXFRVeaa2keP0ozkb2pwrhKmEZE2Pj3F3c/5HklaVt/aNNix23w==</DQ>
<InverseQ>PH6IVe1Ccx5NP8o+NrNCyGxXXIjRGlbqX7lN5R4TysMCbLnYdaqApNv518NeO57f3zK5ZyeZPk7gHMe/i1U4Aw==</InverseQ>
<D>IBf7g7kUiIbvz5hPqN/kbQqR7/s0aRPAxGP1E0eV41fJYihQu9G04TEzeReRaHy2TkOixL0edB8O0jG7iCIDadJ9Hg2ygjj/EMq20U2BvjEGQE1AQzFuSLoRLA5lqqh81BBTShEZ8ti6rMGVM872GAc0HmvzskxQNaDEXp9zoN0=</D>
</RSAKeyValue>");
		
		// Security
		Lobby.InitializeSecurity(true);
		
		// Authoritative
		//AccountManager.Master.isAuthoritative = true;
		
		// Add ourselves as listeners for when accounts log in or out.
		AccountManager.OnAccountLoggedIn += OnAccountLoggedIn;
		AccountManager.OnAccountLoggedOut += OnAccountLoggedOut;
		AccountManager.OnAccountRegistered += OnAccountRegistered;
		
		// Lobby connect
		Lobby.OnPeerConnected += OnPeerConnected;
		
		// Send queue stats
		InvokeRepeating("SendQueueStats", 1.0f, 1.0f);
	}

	void OnSecurityInitialized(LobbyPeer peer) {
		//XDebug.Log ("Initialized security for peer " + peer);
	}
	
	// Peer connected
	void OnPeerConnected(LobbyPeer peer) {
		XDebug.Log("Peer connected: " + peer);
		Lobby.RPC("VersionNumber", peer, serverVersionNumber);
	}
	
	// A new game server has finished starting up
	void uZone_OnInstanceStarted(uZone.GameInstance instance) {
		// TODO: Use a dictionary
		
		// Pick the match this instance has been started for
		foreach(Match match in Match.matchesWaitingForServer) {
			if(match.requestId == instance.requestId) {
				match.StartPlayingOn(instance);
				return;
			}
		}
	}
	
	// Account login
	void OnAccountLoggedIn(Account account) {
		XDebug.Log("Account '<color=yellow>" + account.name + "</color>' logged in.");
		
		// Save the reference in a dictionary
		LobbyPlayer lobbyPlayer = new LobbyPlayer(account);
		
		// Async: Retrieve the player information
		StartCoroutine(lobbyGameDB.GetPlayerName(lobbyPlayer.account.id.value, data => {
			if(data == null) {
				Lobby.RPC("AskPlayerName", lobbyPlayer.peer);
			} else {
				lobbyPlayer.name = data;
				Lobby.RPC("ReceivePlayerInfo", lobbyPlayer.peer, lobbyPlayer.account.id.value, lobbyPlayer.name);
				LobbyServer.OnReceivePlayerName(lobbyPlayer);
			}
		}));
		StartCoroutine(lobbyGameDB.GetPlayerStats(lobbyPlayer));
		StartCoroutine(lobbyGameDB.GetCharacterStats(lobbyPlayer));
		StartCoroutine(lobbyGameDB.GetInputSettings(lobbyPlayer));
		
		//StartCoroutine(LobbyGameDB.GetAccountRegistrationDate(lobbyPlayer));
		
		// Async: Set last login date
		StartCoroutine(lobbyGameDB.SetLastLoginDate(lobbyPlayer, System.DateTime.UtcNow));
	}
	
	// Account logout
	void OnAccountLoggedOut(Account account) {
		//XDebug.Log("'" + account.name + "' logged out.");
		
		LobbyPlayer player = LobbyPlayer.accountIdToLobbyPlayer[account.id.value];
		RemovePlayer(player);
	}
	
	// Account registered
	void OnAccountRegistered(Account account) {
		StartCoroutine(lobbyGameDB.SetAccountRegistrationDate(account.id.value, System.DateTime.UtcNow));
	}
	
	// Once we have the player name, let him join the channel
	public static void OnReceivePlayerName(LobbyPlayer player) {
		GameDB.accountIdToName[player.account.id.value] = player.name;
		LobbyServer.globalChannel.AddPlayer(player);
		XDebug.Log("<color=yellow><b>" + player.name + "</b></color> is online.");
	}
	
	// Once we have the guild ID list, send it to the player
	public static void OnReceiveGuildIdList(LobbyPlayer player) {
		//string guildListString = Jboy.Json.WriteObject(player.guildIdList);
		Lobby.RPC("ReceiveGuildIdList", player.peer, player.guildIdList.ToArray(), true);
	}
	
	// uZone errors
	void uZone_OnError(uZone.ErrorCode error) {
		XDebug.LogWarning ("uZone error code: " + error);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void PlayerNameChange(string newName, LobbyMessageInfo info) {
		// Length minimum
		if(newName.Length < 2)
			return;
		
		// Get the account
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		// Change name
		XDebug.Log("Account " + lobbyPlayer.account.id.value + " has requested to change his player name to '" + newName + "'");
		StartCoroutine(lobbyGameDB.SetPlayerName(lobbyPlayer, newName));
	}
	
	[RPC]
	void EnterQueue(byte playersPerTeam, LobbyMessageInfo info) {
		// Check for correct team size
		if(playersPerTeam > 5)
			return;
		
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		// Do we have ranking information?
		if(lobbyPlayer.stats == null)
			return;
		
		// Add the player to the queue
		queue[playersPerTeam - 1].AddPlayer(lobbyPlayer);
		
		// Let the player know he entered the queue
		XDebug.Log("Added '" + lobbyPlayer.name + "' to " + playersPerTeam + "v" + playersPerTeam + " queue");
		Lobby.RPC("EnteredQueue", lobbyPlayer.peer, playersPerTeam);
	}
	
	[RPC]
	void LeaveQueue(LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		// Make the player leave the queue
		lobbyPlayer.LeaveQueue();
		
		// Let the player know he left the queue
		XDebug.Log("'" + lobbyPlayer.name + "' left the queue");
		Lobby.RPC("LeftQueue", lobbyPlayer.peer);
	}
	
	[RPC]
	void ReturnedFromMatch(LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		XDebug.Log("Player '" + lobbyPlayer.name + "' returned from a match");
		
		// A player just returned from a match, we'll send him queue infos again
		lobbyPlayer.inMatch = false;
		
		// Send him his new stats
		StartCoroutine(lobbyGameDB.GetPlayerStats(lobbyPlayer));
		
		// Send him the chat members again to prevent wrong status info
		foreach(var channel in lobbyPlayer.channels) {
			channel.SendMemberListToPlayer(lobbyPlayer);
		}
	}
	
	[RPC]
	void RankingListRequest(LobbyMessageInfo info) {
		uint maxPlayerCount = 10;
		
		//XDebug.Log("Retrieving top " + maxPlayerCount + " ranks");
		StartCoroutine(lobbyGameDB.GetTopRanks(maxPlayerCount, info.sender));
	}
	
	[RPC]
	void GuildIdListRequest(LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		if(lobbyPlayer.guildIdList == null) {
			StartCoroutine(lobbyGameDB.GetGuildIdList(lobbyPlayer));
		} else {
			OnReceiveGuildIdList(lobbyPlayer);
		}
	}
	
	[RPC]
	IEnumerator GuildInfoRequest(string guildId, LobbyMessageInfo info) {
		// Get guild info from database
		if(!GameDB.guildIdToGuild.ContainsKey(guildId)) {
			yield return StartCoroutine(lobbyGameDB.GetGuild(guildId));
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
			yield return StartCoroutine(lobbyGameDB.GetGuildMembers(guildId));
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
			yield return StartCoroutine(lobbyGameDB.GetGuildMembers(guildId));
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
			yield return StartCoroutine(lobbyGameDB.GetGuildInvitations(accountIdToInvite, data => {
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
		yield return StartCoroutine(lobbyGameDB.SetGuildInvitations(accountIdToInvite, guildInvitations, data => {
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
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		// Get guild invitations
		if(lobbyPlayer.guildInvitations == null) {
			yield return StartCoroutine(lobbyGameDB.GetGuildInvitations(lobbyPlayer.account.id.value, data => {
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
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		// Get guild invitations
		if(lobbyPlayer.guildInvitations == null) {
			yield return StartCoroutine(lobbyGameDB.GetGuildInvitations(lobbyPlayer.account.id.value, data => {
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
				yield return StartCoroutine(lobbyGameDB.GetGuildMembers(guildId));
			}
			
			var guildMembers = GameDB.guildIdToGuildMembers[guildId];
			guildMembers.Add(new GuildMember(lobbyPlayer.account.id.value, (byte)GuildMember.Rank.Default));
			
			// Set guild members
			yield return StartCoroutine(lobbyGameDB.SetGuildMembers(guildId, guildMembers));
			
			// Get guild ID list
			if(lobbyPlayer.guildIdList == null) {
				yield return StartCoroutine(lobbyGameDB.GetGuildIdList(lobbyPlayer));
			}
			
			// Add to guild ID list
			lobbyPlayer.guildIdList.Add(guildId);
			
			// Set guild ID list
			yield return StartCoroutine(lobbyGameDB.SetGuildIdList(lobbyPlayer));
		}
		
		// Remove guild from invitation list
		lobbyPlayer.guildInvitations.Remove(guildId);
		
		// Set guild invitations
		yield return StartCoroutine(lobbyGameDB.SetGuildInvitations(lobbyPlayer.account.id.value, lobbyPlayer.guildInvitations, data => {
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
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		if(name.Length > GameDB.maxGuildNameLength) {
			Lobby.RPC("GuildNameLengthError", info.sender);
			yield break;
		}
		
		if(tag.Length > GameDB.maxGuildTagLength) {
			Lobby.RPC("GuildTagLengthError", info.sender);
			yield break;
		}
		
		// Store new guild in database
		yield return StartCoroutine(lobbyGameDB.PutGuild(new Guild(name, tag, lobbyPlayer.account.id.value), lobbyPlayer));
		
		// Store new guild membership in database
		yield return StartCoroutine(lobbyGameDB.SetGuildIdList(lobbyPlayer));
		
		// Let the player know that it worked
		Lobby.RPC("GuildCreationSuccess", info.sender);
		OnReceiveGuildIdList(lobbyPlayer);
	}
	
	[RPC]
	void ClientCharacterStats(CharacterStats charStats, LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		if(charStats.totalStatPointsUsed > charStats.maxStatPoints) {
			XDebug.LogWarning("Detected character stat points hack on player '" +lobbyPlayer.name  + "'");
			return;
		}
		
		XDebug.Log("Player '" + lobbyPlayer.name + "' sent new character stats " + charStats.ToString());
		StartCoroutine(lobbyGameDB.SetCharacterStats(lobbyPlayer, charStats));
	}
	
	[RPC]
	void ClientInputSettings(string inputSettingsString, LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		XDebug.Log("Player '" + lobbyPlayer.name + "' sent new input settings");
		InputSettings inputSettings = Jboy.Json.ReadObject<InputSettings>(inputSettingsString);
		StartCoroutine(lobbyGameDB.SetInputSettings(lobbyPlayer, inputSettings));
	}
	
	[RPC]
	void ClientChat(string channelName, string msg, LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		XDebug.Log("[" + channelName + "][" + lobbyPlayer.name + "] '" + msg + "'");
		
		if(LobbyChatChannel.channels.ContainsKey(channelName)) {
			var channel = LobbyChatChannel.channels[channelName];
			
			// Channel member?
			if(channel.members.Contains(lobbyPlayer)) {
				if(!ProcessLobbyChatCommands(lobbyPlayer, msg))
					channel.BroadcastMessage(lobbyPlayer.name, msg);
			}
		}
	}
	
	bool ProcessLobbyChatCommands(LobbyPlayer lobbyPlayer, string msg) {
		if(msg.StartsWith("//ginvite ")) {
			/*StartCoroutine(lobbyGameDB.GetAccountIdByPlayerName(msg.Split(' ')[1], data => {
				XDebug.Log ("ginvite: " + data);
			}));*/
			return true;
		}
		
		return false;
	}
}
