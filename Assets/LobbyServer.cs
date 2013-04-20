using UnityEngine;
using uLobby;
using uGameDB;
using uZone;
using System.Collections;
using System.Collections.Generic;

public class LobbyServer : MonoBehaviour {
	public static bool uZoneConnected = false;
	public static int uZoneNodeCount;
	public static string gameName = "bomWithFlags";
	public static LobbyChatChannel globalChannel = new LobbyChatChannel("Global");
	
	// Database component
	public static LobbyGameDB lobbyGameDB;
	public static TraitsDB traitsDB;
	public static SettingsDB settingsDB;
	
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
		uZone.InstanceManager.AddListener(this.gameObject);
		uZone.InstanceManager.Initialise();
		uZone.InstanceManager.Connect(uZoneHost, uZonePort);
		
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
		// Get DB components
		lobbyGameDB = this.GetComponent<LobbyGameDB>();
		traitsDB = this.GetComponent<TraitsDB>();
		settingsDB = this.GetComponent<SettingsDB>();
		
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
	public static LobbyPlayer GetLobbyPlayer(LobbyMessageInfo info) {
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
		AccountManager.Master.isAuthoritative = true;
		
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
		StartCoroutine(traitsDB.GetCharacterStats(lobbyPlayer));
		StartCoroutine(settingsDB.GetInputSettings(lobbyPlayer));
		
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
	
	// Account register failed
	/*void OnRegisterFailed(string accountName, AccountError error) {
		// Bug in uLobby: We need to call this explicitly on the client
		Lobby.RPC("_RPCOnRegisterAccountFailed", info.sender, registerReq.result);
	}*/
	
	// Once we have the player name, let him join the channel
	public static void OnReceivePlayerName(LobbyPlayer player) {
		GameDB.accountIdToName[player.account.id.value] = player.name;
		LobbyServer.globalChannel.AddPlayer(player);
		XDebug.Log("<color=yellow><b>" + player.name + "</b></color> is online.");
	}
	
	// uZone connection established
	void uZone_OnConnected(string id) {
		XDebug.Log("Connected to uZone (ID: " + id + ").");
		
		LobbyServer.uZoneConnected = true;
		uZone.InstanceManager.ListAvailableNodes();
	}
	
	// uZone node connection established
	void uZone_OnNodeConnected(uZone.Node node) {
		XDebug.Log("Connected to uZone node (" + node.ToString() + ")");
		
		// Start matchmaking after downtime of uZone nodes
		if(queue != null && LobbyServer.uZoneNodeCount == 0) {
			foreach(var q in queue) {
				q.MakeMatchesBasedOnRanking();
			}
		}
		
		LobbyServer.uZoneNodeCount += 1;
	}
	
	// uZone node connection lost
	void uZone_OnNodeDisconnected(string id) {
		XDebug.LogError("Lost connection to uZone node (NodeID: " + id + ")");
		LobbyServer.uZoneNodeCount -= 1;
	}
	
	// uZone node list
	void uZone_OnNodeListReceived(List<uZone.Node> newNodeList) {
		XDebug.Log("Received uZone node list (" + newNodeList.Count + " online).");
		
		foreach(var node in newNodeList) {
			XDebug.Log(node.ToString());
		}
		
		LobbyServer.uZoneNodeCount = newNodeList.Count;
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
	
	// uZone errors
	void uZone_OnError(uZone.ErrorCode error) {
		XDebug.LogWarning ("uZone error code: " + error);
	}
	
	// --------------------------------------------------------------------------------
	// Account Management RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	IEnumerator LobbyRegisterAccount(string accountName, byte[] passwordHash, string email, LobbyMessageInfo info) {
		// Validate data
		if(!Validator.accountName.IsMatch(accountName))
			yield break;
		
		if(!Validator.email.IsMatch(email))
			yield break;
		
		// Check if email has already been registered
		bool emailExists = false;
		
		yield return StartCoroutine(lobbyGameDB.GetAccountIdByEmail(email, data => {
			if(data != null) {
				emailExists = true;
			}
		}));
		
		if(emailExists) {
			Lobby.RPC("EmailAlreadyExists", info.sender);
			yield break;
		}
		
		// Register account in uLobby
		byte[] customData = new byte[0];
		uLobby.Request<Account> registerReq = AccountManager.Master.RegisterAccount(accountName, passwordHash, customData);
		yield return registerReq.WaitUntilDone();
		
		// Bug in uLobby: We need to call this explicitly on the client
		if(!registerReq.isSuccessful) {
			AccountException exception = (AccountException)registerReq.exception;
			AccountError error = exception.error;
			
			Lobby.RPC("_RPCOnRegisterAccountFailed", info.sender, accountName, error);
			yield break;
		}
		
		// Set email for the account
		Account account = registerReq.result;
		yield return StartCoroutine(lobbyGameDB.SetEmail(account.id.value, email, data => {
			// ...
		}));
		
		// Bug in uLobby: We need to call this explicitly on the client
		Lobby.RPC("_RPCOnAccountRegistered", info.sender, account);
	}
	
	[RPC]
	IEnumerator LobbyAccountLogIn(string accountName, byte[] passwordHash, LobbyMessageInfo info) {
		uLobby.Request<Account> loginReq = AccountManager.Master.LogIn(info.sender, accountName, passwordHash);
		yield return loginReq.WaitUntilDone();
		
		if(!loginReq.isSuccessful) {
			AccountException exception = (AccountException)loginReq.exception;
			AccountError error = exception.error;
			
			// Bug in uLobby: We need to call this explicitly on the client
			Lobby.RPC("_RPCOnLogInFailed", info.sender, accountName, error);
			yield break;
		}
	}
	
	[RPC]
	IEnumerator LobbyAccountLogOut(LobbyMessageInfo info) {
		uLobby.Request req = AccountManager.Master.LogOut(info.sender);
		yield return req.WaitUntilDone();
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	IEnumerator PlayerNameChange(string newName, LobbyMessageInfo info) {
		// Validate data
		if(!Validator.playerName.IsMatch(newName))
			yield break;
		
		// Check if name exists already
		bool nameExists = false;
		
		yield return StartCoroutine(lobbyGameDB.GetAccountIdByPlayerName(newName, data => {
			if(data != null) {
				nameExists = true;
			}
		}));
		
		if(nameExists) {
			Lobby.RPC("PlayerNameAlreadyExists", info.sender);
			yield break;
		}
		
		// Get the account
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		// Change name
		XDebug.Log("Account " + lobbyPlayer.account.id.value + " has requested to change his player name to '" + newName + "'");
		StartCoroutine(lobbyGameDB.SetPlayerName(lobbyPlayer, newName));
	}
	
	[RPC]
	void EnterQueue(byte playersPerTeam, LobbyMessageInfo info) {
		// Check for correct team size
		if(playersPerTeam == 0 || playersPerTeam > 5)
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
}
