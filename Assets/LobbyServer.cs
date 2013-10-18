using UnityEngine;
using uLobby;
using uGameDB;
using uZone;
using System.Collections;
using System.Collections.Generic;

public class LobbyServer : MonoBehaviour {
	public static LobbyServer instance = null;
	public static bool uZoneConnected = false;
	public static int uZoneNodeCount;
	public static string gameName = "bomWithFlags";
	public static LobbyChatChannel globalChannel = new LobbyChatChannel("Global");
	public static LobbyChatChannel announceChannel = new LobbyChatChannel("Announcement");
	
	// Database component
	public static LobbyGameDB lobbyGameDB;
	public static AccessLevelsDB accessLevelsDB;
	public static SkillBuildsDB skillBuildsDB;
	public static TraitsDB traitsDB;
	public static GuildsDB guildsDB;
	public static ArtifactsDB artifactsDB;
	public static CharacterCustomizationDB characterCustomizationDB;
	public static SettingsDB settingsDB;
	public static IPInfoDB ipInfoDB;
	
	public int maxConnections = 1024;
	public int listenPort = 1310;
	public string databaseHost = "battle-of-mages.com";
	public int databasePort = 8087;
	public string uZoneHost = "127.0.0.1";
	public int uZonePort = 12345;
	
	private Version serverVersion;
	
	private LobbyQueue[] queue;
	
	private int loggedPlayerCount;
	
	void Awake() {
		LobbyServer.instance = this;
		
		// Limit frame rate
		Application.targetFrameRate = 60;
		//uLinkTargetFrameRateFix.SetTargetFrameRate(20);
		
		// Create log view scripts
		CreateLogViewScripts();
		
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
		accessLevelsDB = this.GetComponent<AccessLevelsDB>();
		traitsDB = this.GetComponent<TraitsDB>();
		artifactsDB = this.GetComponent<ArtifactsDB>();
		settingsDB = this.GetComponent<SettingsDB>();
		ipInfoDB = this.GetComponent<IPInfoDB>();
		skillBuildsDB = this.GetComponent<SkillBuildsDB>();
		guildsDB = this.GetComponent<GuildsDB>();
		characterCustomizationDB = this.GetComponent<CharacterCustomizationDB>();
		
		// Version number
		serverVersion = this.GetComponent<Version>();
		
		// Event handlers
		Lobby.OnLobbyInitialized += OnLobbyInitialized;
		Lobby.OnSecurityInitialized += OnSecurityInitialized;
		
		// Make this class listen to Lobby events
		Lobby.AddListener(this);
		
		// Initialize the lobby
		LogManager.General.Log("Initializing lobby on port " + listenPort + " with a maximum of " + maxConnections + " players.");
		Lobby.InitializeLobby(maxConnections, listenPort, databaseHost, databasePort);
	}
	
	void CreateLogViewScripts() {
		string cat = "#!/bin/sh\ncat ";
		string tail = "#!/bin/sh\ntail -f ";
		
		System.IO.File.WriteAllText("./tail-general.sh", tail + LogManager.General.filePath);
		System.IO.File.WriteAllText("./tail-online.sh", tail + LogManager.Online.filePath);
		System.IO.File.WriteAllText("./tail-db.sh", tail + LogManager.DB.filePath);
		System.IO.File.WriteAllText("./tail-chat.sh", tail + LogManager.Chat.filePath);
		
		System.IO.File.WriteAllText("./cat-general.sh", cat + LogManager.General.filePath);
		System.IO.File.WriteAllText("./cat-online.sh", cat + LogManager.Online.filePath);
		System.IO.File.WriteAllText("./cat-db.sh", cat + LogManager.DB.filePath);
		System.IO.File.WriteAllText("./cat-chat.sh", cat + LogManager.Chat.filePath);
	}
	
	void RemovePlayer(LobbyPlayer player) {
		// Remove the player from the queue he was in
		if(player.queue != null)
			player.queue.RemovePlayer(player);
		
		// Remove game instance associations
		if(player.inTown)
			player.town = null;
		
		if(player.inMatch)
			player.match = null;
		
		// Remove the reference from the dictionary
		LobbyPlayer.accountIdToLobbyPlayer.Remove(player.accountId);
		
		// Remove the player from the global list
		LobbyPlayer.list.Remove(player);
		
		// Remove the player from all chat channels
		foreach(var channel in new List<LobbyChatChannel>(player.channels)) {
			channel.RemovePlayer(player);
		}
		
		// Log it
		LogManager.Online.Log("'" + player.name + "' logged out. (Peer: " + player.peer + ", Acc: '" + player.account.name + "', AccID: '" + player.accountId + "')");
	}
	
	// We send players information about the queues each second
	void SendQueueStats() {
		var offlinePlayers = new List<LobbyPlayer>();
		int playerCount = LobbyPlayer.list.Count;
		
		if(loggedPlayerCount != playerCount) {
			LogManager.Spam.Log("SendQueueStats [" + playerCount + " players] started.");
		}
		
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
				LogManager.General.Log("Couldn't send queue data to player '" + player.name + "' from account '" + player.account.name + "'");
			}
		}
		
		// Clear offline players
		foreach(LobbyPlayer player in offlinePlayers) {
			RemovePlayer(player);
		}
		
		if(loggedPlayerCount != playerCount) {
			LogManager.Spam.Log("SendQueueStats [" + playerCount + " players] finished.");
			loggedPlayerCount = playerCount;
		}
	}
	
	// Sends a system message
	public static void SendSystemMsg(LobbyPlayer player, string msg) {
		Lobby.RPC("Chat", player.peer, "System", "", msg);
	}
	
	// Sends data about the account to any player
	void SendPublicAccountInfo(string accountId, LobbyPlayer toPlayer) {
		LobbyPlayer player = GetLobbyPlayer(accountId);
		
		// Character customization
		StartCoroutine(characterCustomizationDB.GetCharacterCustomization(accountId, data => {
			if(data == null) {
				if(player == toPlayer)
					Lobby.RPC("CustomizeCharacter", toPlayer.peer, accountId);
			} else {
				if(player != null)
					player.custom = data;
				Lobby.RPC("ReceiveCharacterCustomization", toPlayer.peer, accountId, data);
			}
		}));
		
		// Name
		StartCoroutine(lobbyGameDB.GetPlayerName(accountId, data => {
			if(data == null) {
				if(player == toPlayer)
					Lobby.RPC("AskPlayerName", toPlayer.peer);
			} else {
				Lobby.RPC("ReceivePlayerInfo", toPlayer.peer, accountId, data);
				
				if(player == toPlayer) {
					if(player.name == null || player.name == "") {
						player.name = data;
						LobbyServer.OnReceivePlayerName(player);
					}
				}
			}
		}));
		
		// Skill build
		StartCoroutine(skillBuildsDB.GetSkillBuild(accountId, data => {
			if(data == null) {
				Lobby.RPC("ReceiveSkillBuild", toPlayer.peer, accountId, SkillBuild.GetStarterBuild());
			} else {
				Lobby.RPC("ReceiveSkillBuild", toPlayer.peer, accountId, data);
			}
		}));
		
		// Stats
		StartCoroutine(lobbyGameDB.GetPlayerStats(accountId, data => {
			if(data == null)
				data = new PlayerStats();
			
			// Assign stats
			if(player != null)
				player.stats = data;
			
			// Send the stats to the player
			Lobby.RPC("ReceivePlayerStats", toPlayer.peer,
				accountId,
				Jboy.Json.WriteObject(data)
			);
		}));
		
		// Character stats
		StartCoroutine(traitsDB.GetCharacterStats(accountId, data => {
			if(data == null)
				data = new CharacterStats();
			
			if(player != null)
				player.charStats = data;
			
			Lobby.RPC("ReceiveCharacterStats", toPlayer.peer, accountId, data);
		}));
		
		// Artifact inventory
		StartCoroutine(artifactsDB.GetArtifactInventory(accountId, data => {
			if(data == null)
				data = new ArtifactInventory();
			
			if(player != null)
				player.artifactInventory = data;
			
			Lobby.RPC("ReceiveArtifactInventory", toPlayer.peer, accountId, Jboy.Json.WriteObject(data));
		}));
		
		// Artifact tree
		StartCoroutine(artifactsDB.GetArtifactTree(accountId, data => {
			if(data == null)
				data = ArtifactTree.GetStarterArtifactTree();
			
			if(player != null)
				player.artifactTree = data;
			
			Lobby.RPC("ReceiveArtifactTree", toPlayer.peer, accountId, Jboy.Json.WriteObject(data));
		}));
		
		// View profile
		Lobby.RPC("ViewProfile", toPlayer.peer, accountId);
	}
	
	// Start town servers
	protected void StartTownServers() {
		foreach(var mapName in MapManager.towns) {
			new LobbyTown(mapName).Register();
		}
	}
	
	// Stop town servers
	protected void StopTownServers() {
		foreach(var townInstance in LobbyTown.running) {
			uZone.InstanceManager.StopInstance(townInstance.instance.id);
		}
	}
	
	// Returns a player to his town
	public void ReturnPlayerToTown(LobbyPlayer player) {
		// Map name
		string playerMap = "Nubek";
		
		// Start new town server if needed
		LobbyTown townInstance = null;
		if(!LobbyTown.mapNameToInstances.ContainsKey(playerMap) || LobbyTown.mapNameToInstances[playerMap].Count == 0) {
			townInstance = new LobbyTown(playerMap);
			townInstance.Register();
		} else {
			var lobbyGameInstance = LobbyTown.mapNameToInstances[playerMap][0];
			townInstance = (LobbyTown)lobbyGameInstance;
		}
		
		// Connect the player once the instance is ready
		StartCoroutine(player.ConnectToGameInstanceDelayed(townInstance));
	}
	
	// Gets the lobby player by the supplied message info
	public static LobbyPlayer GetLobbyPlayer(LobbyMessageInfo info) {
		Account account = AccountManager.Master.GetLoggedInAccount(info.sender);
		return LobbyPlayer.accountIdToLobbyPlayer[account.id.value];
	}
	
	// Gets the lobby player by the account ID
	public static LobbyPlayer GetLobbyPlayer(string accountId) {
		if(!LobbyPlayer.accountIdToLobbyPlayer.ContainsKey(accountId))
			return null;
		
		return LobbyPlayer.accountIdToLobbyPlayer[accountId];
	}
	
#region Callbacks
	// --------------------------------------------------------------------------------
	// Callbacks
	// --------------------------------------------------------------------------------
	
	// Once we have the player name, let him join the channel
	public static void OnReceivePlayerName(LobbyPlayer player) {
		GameDB.accountIdToName[player.accountId] = player.name;
		
		LogManager.Online.Log("'" + player.name + "' logged in. (Peer: " + player.peer + ", Acc: '" + player.account.name + "', AccID: '" + player.accountId + "')");
	}
	
	// Account login
	void OnAccountLoggedIn(Account account) {
		// Save the reference in a dictionary
		LobbyPlayer player = new LobbyPlayer(account);
		
		// Disconnected already?
		// This can happen if the database takes too much time to respond.
		if(player.peer.type == LobbyPeerType.Disconnected)
			return;
		
		LogManager.General.Log("Account '" + account.name + "' logged in.");
		
		//StartCoroutine(accessLevelsDB.SetAccessLevel(player, AccessLevel.Admin));
		
		// Async: Retrieve the player information
		SendPublicAccountInfo(player.accountId, player);
		
		// Others
		StartCoroutine(settingsDB.GetInputSettings(player));
		StartCoroutine(accessLevelsDB.GetAccessLevel(player));
		
		//StartCoroutine(LobbyGameDB.GetAccountRegistrationDate(player));
		
		// Async: Set last login date
		StartCoroutine(lobbyGameDB.SetLastLoginDate(player, System.DateTime.UtcNow));
		
		// Save IP
		string ip = player.peer.endpoint.Address.ToString();
		string accountId = player.accountId;
		
		// Get and set account list for that IP
		StartCoroutine(ipInfoDB.GetAccounts(
			ip,
			data => {
				List<string> accounts;
				
				if(data == null) {
					accounts = new List<string>();
				} else {
					accounts = new List<string>(data);
				}
				
				// Save new account id
				if(accounts.IndexOf(accountId) == -1)
					accounts.Add(accountId);
				
				StartCoroutine(ipInfoDB.SetAccounts(
					ip,
					accounts.ToArray(),
					ignore => {}
				));
			}
		));
		
		// Save country
		if(IPInfoServer.ipToCountry.ContainsKey(ip)) {
			StartCoroutine(ipInfoDB.SetCountry(
				player.accountId,
				IPInfoServer.ipToCountry[ip],
				data => {
					if(data != null) {
						IPInfoServer.accountIdToCountry[player.accountId] = data;
					}
				}
			));
		}
	}
	
	// Account logout
	void OnAccountLoggedOut(Account account) {
		//LogManager.General.Log("'" + account.name + "' logged out.");
		
		LobbyPlayer player = LobbyPlayer.accountIdToLobbyPlayer[account.id.value];
		RemovePlayer(player);
	}
	
	// Lobby initialized
	void OnLobbyInitialized() {
		LogManager.General.Log("Successfully initialized lobby.");
		
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
		Lobby.OnPeerDisconnected += OnPeerDisconnected;
		
		// Send queue stats
		InvokeRepeating("SendQueueStats", 1.0f, 1.0f);
	}

	void OnSecurityInitialized(LobbyPeer peer) {
		//Debug.Log ("Initialized security for peer " + peer);
	}
	
	// Peer connected
	void OnPeerConnected(LobbyPeer peer) {
		LogManager.Online.Log("Peer connected: " + peer);
		Lobby.RPC("VersionNumber", peer, serverVersion.versionNumber);
		
		// Look up country by IP
		StartCoroutine(IPInfoServer.GetCountryByIP(peer.endpoint.Address.ToString()));
	}
	
	// Peer disconnected
	void OnPeerDisconnected(LobbyPeer peer) {
		LogManager.Online.Log("Peer disconnected: " + peer);
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
	
	// On application quit
	void OnApplicationQuit() {
		LogManager.General.Close();
		LogManager.Online.Close();
		LogManager.Chat.Close();
		LogManager.DB.Close();
	}
#endregion
	
#region uZone
	// uZone connection established
	void uZone_OnConnected(string id) {
		LogManager.General.Log("Connected to uZone (ID: " + id + ").");
		
		LobbyServer.uZoneConnected = true;
		uZone.InstanceManager.ListAvailableNodes();
	}
	
	// uZone node connection established
	void uZone_OnNodeConnected(uZone.Node node) {
		LogManager.General.Log("Connected to uZone node (" + node.ToString() + ")");
		
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
		LogManager.General.LogError("Lost connection to uZone node (NodeID: " + id + ")");
		LobbyServer.uZoneNodeCount -= 1;
	}
	
	// uZone node list
	void uZone_OnNodeListReceived(List<uZone.Node> newNodeList) {
		LogManager.General.Log("Received uZone node list (" + newNodeList.Count + " online).");
		
		foreach(var node in newNodeList) {
			LogManager.General.Log(node.ToString());
		}
		
		LobbyServer.uZoneNodeCount = newNodeList.Count;
		
		// Start town servers
		//StartTownServers();
	}
	
	// A new game server has finished starting up
	void uZone_OnInstanceStarted(uZone.GameInstance instance) {
		LogManager.General.Log("uZone instance started: " + instance.ToString());
		
		// Pick the match this instance has been started for
		var instanceId = instance.requestId;
		
		if(LobbyMatch.requestIdToInstance.ContainsKey(instanceId)) {
			LobbyMatch.requestIdToInstance[instanceId].StartPlayingOn(instance);
			return;
		}
		
		if(LobbyTown.requestIdToInstance.ContainsKey(instanceId)) {
			LobbyTown.requestIdToInstance[instanceId].StartPlayingOn(instance);
			return;
		}
		
		/*foreach(var match in LobbyMatch.waitingForServer) {
			if(match.requestId == instance.requestId) {
				match.StartPlayingOn(instance);
				return;
			}
		}*/
	}
	
	// A game server stopped running
	void uZone_OnInstanceStopped(string id) {
		LogManager.General.Log("Instance ID '" + id + "' stopped running.");
		
		if(LobbyMatch.idToInstance.ContainsKey(id)) {
			LobbyMatch.idToInstance[id].Unregister();
			return;
		}
		
		if(LobbyTown.idToInstance.ContainsKey(id)) {
			LobbyTown.idToInstance[id].Unregister();
			return;
		}
	}
	
	// uZone errors
	void uZone_OnError(uZone.ErrorCode error) {
		LogManager.General.LogWarning("uZone error code: " + error);
	}
#endregion
	
#region RPCs
	// --------------------------------------------------------------------------------
	// Account Management RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	IEnumerator LobbyRegisterAccount(string email, string password, LobbyMessageInfo info) {
		// Validate data
		// Password is modified at this point anyway, no need to check it
		if(!Validator.email.IsMatch(email) && !GameDB.IsTestAccount(email))
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
		uLobby.Request<Account> registerReq = AccountManager.Master.RegisterAccount(email, password);
		yield return registerReq.WaitUntilDone();
		
		// Bug in uLobby: We need to call this explicitly on the client
		if(!registerReq.isSuccessful) {
			AccountException exception = (AccountException)registerReq.exception;
			AccountError error = exception.error;
			
			Lobby.RPC("_RPCOnRegisterAccountFailed", info.sender, email, error);
			yield break;
		}
		
		// Set email for the account
		Account account = registerReq.result;
		yield return StartCoroutine(lobbyGameDB.SetEmail(account.id.value, email, data => {
			// ...
		}));
		
		// Bug in uLobby: We need to call this explicitly on the client
		Lobby.RPC("_RPCOnAccountRegistered", info.sender, account);
		
		// Log it
		LogManager.General.Log("New account has been registered: E-Mail: '" + email + "'");
	}
	
	[RPC]
	IEnumerator LobbyAccountLogIn(string email, string password, LobbyMessageInfo info) {
		uLobby.Request<Account> loginReq = AccountManager.Master.LogIn(info.sender, email, password);
		yield return loginReq.WaitUntilDone();
		
		if(!loginReq.isSuccessful) {
			AccountException exception = (AccountException)loginReq.exception;
			AccountError error = exception.error;
			
			// Bug in uLobby: We need to call this explicitly on the client
			Lobby.RPC("_RPCOnLogInFailed", info.sender, email, error);
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
		// Prettify to be safe
		newName = newName.PrettifyPlayerName();
		
		// Validate data
		if(!Validator.playerName.IsMatch(newName))
			yield break;
		
		// Check if name exists already
		yield return StartCoroutine(lobbyGameDB.GetAccountIdByPlayerName(newName, data => {
			if(data != null) {
				Lobby.RPC("PlayerNameAlreadyExists", info.sender, newName);
			} else {
				// Get the account
				LobbyPlayer player = GetLobbyPlayer(info);
				
				// Change name
				LogManager.General.Log("Account " + player.accountId + " has requested to change its player name to '" + newName + "'");
				StartCoroutine(lobbyGameDB.SetPlayerName(player, newName));
			}
		}));
	}
	
	[RPC]
	IEnumerator PlayerNameExists(string newName, LobbyMessageInfo info) {
		yield return StartCoroutine(lobbyGameDB.GetAccountIdByPlayerName(newName, data => {
			if(data != null) {
				Lobby.RPC("PlayerNameAlreadyExists", info.sender, newName);
			} else {
				Lobby.RPC("PlayerNameFree", info.sender, newName);
			}
		}));
	}
	
	[RPC]
	IEnumerator AccountPasswordChange(string newPassword, LobbyMessageInfo info) {
		// Get the account
		LobbyPlayer player = GetLobbyPlayer(info);
		
		// Change name
		LogManager.General.Log("Account " + player.accountId + " has requested to change its password hash.");
		yield return StartCoroutine(lobbyGameDB.SetPassword(player, newPassword));
		
		Lobby.RPC("PasswordChangeSuccess", player.peer);
	}
	
	[RPC]
	void EnterQueue(byte playersPerTeam, LobbyMessageInfo info) {
		// Check for correct team size
		if(playersPerTeam == 0 || playersPerTeam > 5)
			return;
		
		LobbyPlayer player = GetLobbyPlayer(info);
		
		// Do we have ranking information?
		if(player.stats == null)
			return;
		
		if(player.inMatch)
			return;
		
		// Add the player to the queue
		if(queue[playersPerTeam - 1].AddPlayer(player)) {
			// Let the player know he entered the queue
			LogManager.General.Log("Added '" + player.name + "' to " + playersPerTeam + "v" + playersPerTeam + " queue");
			Lobby.RPC("EnteredQueue", player.peer, playersPerTeam);
		}
	}
	
	[RPC]
	void LeaveQueue(LobbyMessageInfo info) {
		LobbyPlayer player = GetLobbyPlayer(info);
		
		// Make the player leave the queue
		if(player.LeaveQueue()) {
			// Let the player know he left the queue
			LogManager.General.Log("'" + player.name + "' left the queue");
			Lobby.RPC("LeftQueue", player.peer);
		}
	}
	
	[RPC]
	IEnumerator ViewProfile(string playerName, LobbyMessageInfo info) {
		LobbyPlayer playerRequesting = GetLobbyPlayer(info);
		
		yield return StartCoroutine(lobbyGameDB.GetAccountIdByPlayerName(playerName, data => {
			if(data != null) {
				SendPublicAccountInfo(data, playerRequesting);
			} else {
				Lobby.RPC("ViewProfileError", info.sender, playerName);
			}
		}));
	}
	
	[RPC]
	void Ready(LobbyMessageInfo info) {
		LobbyPlayer player = GetLobbyPlayer(info);
		
		ReturnPlayerToTown(player);
		
		// Chat channels
		LobbyServer.globalChannel.AddPlayer(player);
		LobbyServer.announceChannel.AddPlayer(player);
		
		SendSystemMsg(player, "All alpha testers will receive unique rewards at the end of the Open Alpha.");
		SendSystemMsg(player, "Thanks for testing this game.");
		SendSystemMsg(player, "Type //practice if you'd like to practice.");
	}
	
	void LeaveMatch(LobbyPlayer player) {
		if(!player.inMatch)
			return;
		
		LogManager.General.Log("Player '" + player.name + "' left a match");
		
		// A player just returned from a match, we'll send him queue infos again
		player.match = null;
		
		// Send him the chat members again to prevent wrong status info
		foreach(var channel in player.channels) {
			channel.SendMemberListToPlayer(player);
		}
		
		// Return him to town
		ReturnPlayerToTown(player);
	}
	
	[RPC]
	IEnumerator LeaveInstance(bool gameEnded, LobbyMessageInfo info) {
		LobbyPlayer player = GetLobbyPlayer(info);
		
		if(player.inMatch) {
			LogManager.General.Log("Player '" + player.name + "' returned from a match");
			
			if(gameEnded) {
				// Send him his new stats
				StartCoroutine(lobbyGameDB.GetPlayerStats(player));
				
				// Send him his new artifact inventory
				StartCoroutine(artifactsDB.GetArtifactInventory(player));
				
				// Update ranking list cache
				if(!player.match.updatedRankingList) {
					RankingsServer.instance.StartRankingListCacheUpdate(player.match);
					player.match.updatedRankingList = true;
				}
			}
			
			LeaveMatch(player);
		} else if(player.inTown) {
			player.town = null;
			
			if(AccountManager.Master.IsLoggedIn(player.peer)) {
				yield return AccountManager.Master.LogOut(info.sender).WaitUntilDone();
			}
		}
	}
#endregion
}
