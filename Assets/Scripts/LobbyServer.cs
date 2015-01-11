using UnityEngine;
using uLobby;
using System.Collections;
using System.IO;

public class LobbyServer : SingletonMonoBehaviour<LobbyServer>, Initializable {
	// Settings
	public int maxConnections = 1024;
	public int listenPort = 1310;
	public int frameRate = 20;
	public string privateKeyPath;
	public string loginMessagePath;
	
	private string loginMessage;
	
#region Initialization
	// Init
	public void Init() {
		// System report
		LogManager.System.GenerateReport();
		
		// Limit frame rate
		Application.targetFrameRate = frameRate;

		// Configuration
		Configure();

		// Init
		GameDBConnector.instance.onConnect += StartLobbyServer;
	}

	// ConfigureLobby
	void Configure() {
		// Set new values
		Lobby.config.timeoutDelay = 20f;
		Lobby.config.timeBetweenPings = 5f;
		Lobby.config.handshakeRetriesMaxCount = 5;
		Lobby.config.handshakeRetryDelay = 2.5f;
		
		// Log
		LogManager.System.Log("MTU: " + Lobby.config.maximumTransmissionUnit);
		LogManager.System.Log("Timeout delay: " + Lobby.config.timeoutDelay);
		LogManager.System.Log("Time between pings: " + Lobby.config.timeBetweenPings);
		LogManager.System.Log("Handshake max. retries: " + Lobby.config.handshakeRetriesMaxCount);
		LogManager.System.Log("Handshake retry delay: " + Lobby.config.handshakeRetryDelay);
	}

	// StartLobbyServer
	void StartLobbyServer() {
		// Register event listeners
		Lobby.OnLobbyInitialized += OnLobbyInitialized;
		Lobby.OnPeerConnected += OnPeerConnected;
		Lobby.OnPeerDisconnected += OnPeerDisconnected;
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
		
		// Initialize the lobby
		LogManager.General.Log("Initializing lobby on port " + listenPort + " with a maximum of " + maxConnections + " players.");

		Lobby.InitializeLobby(
			maxConnections,
			listenPort,
			new RiakStorageManager(),
			new RiakAccountManager(),
			new RiakFriendManager()
		);
	}
#endregion

#region Callbacks
	void OnLobbyInitialized() {
		LogManager.General.Log("Successfully initialized lobby.");
		
		// Private key
		LogManager.General.Log("Reading private key file");
		Lobby.privateKey = new PrivateKey(File.ReadAllText(privateKeyPath));
		
		// Login message
		LogManager.General.Log("Reading login message file");
		loginMessage = File.ReadAllText(loginMessagePath);
		
		LogManager.General.Log("Login message: " + loginMessage);
		
		// Security
		LogManager.General.Log("Initializing security");
		Lobby.InitializeSecurity(true);
		
		// Authoritative account manager
		LogManager.General.Log("Setting up account manager");
		AccountManager.Master.isAuthoritative = true;
		
		// Add ourselves as listeners for when accounts log in or out
		AccountManager.OnAccountLoggedIn += OnAccountLoggedIn;
		AccountManager.OnAccountLoggedOut += OnAccountLoggedOut;
		AccountManager.OnAccountRegistered += OnAccountRegistered;
		
		// Try to free up some RAM
		LogManager.General.Log("Freeing up RAM");
		System.GC.Collect();
		Resources.UnloadUnusedAssets();
	}

	// Peer connected
	void OnPeerConnected(LobbyPeer peer) {
		// Log it
		var peerOnlineMsg = "Peer connected: " + peer;
		
		LogManager.General.Log(peerOnlineMsg);
		LogManager.Online.Log(peerOnlineMsg);
		
		// Send current version number to peer
		//Lobby.RPC("VersionNumber", peer, Version.instance.versionNumber);
		
		// Look up country by IP
		//IPInfoServer.GetCountry(peer);
	}
	
	// Peer disconnected
	public void OnPeerDisconnected(LobbyPeer peer) {
		StartCoroutine(RemovePeer(peer));
	}
	
	// RemovePeer
	IEnumerator RemovePeer(LobbyPeer peer) {
		// Log him out
		if(AccountManager.Master.IsLoggedIn(peer)) {
			// Perform logout
			var req = AccountManager.Master.LogOut(peer);
			yield return req.WaitUntilDone();
		}

		// Remove the player from all lists
		LobbyPlayer player;
		
		if(LobbyPlayer.peerToLobbyPlayer.TryGetValue(peer, out player)) {
			// Just to be safe, in case OnAccountLoggedOut failed
			player.Remove();
			
			// Log it
			var msg = string.Format("Removed player: {0}", player);
			
			LogManager.General.Log(msg);
			LogManager.Online.Log(msg);
		}
	}
	
	// Account registered
	void OnAccountRegistered(Account account) {
		// Save registration date in database
		LobbyGameDB.SetAccountRegistrationDate(
			account.id.value,
			System.DateTime.UtcNow
		);
	}
	
	// Account login
	void OnAccountLoggedIn(Account account) {

	}
	
	// Account logout
	void OnAccountLoggedOut(Account account) {
		LobbyPlayer player;

		if(LobbyPlayer.accountIdToLobbyPlayer.TryGetValue(account.id.value, out player)) {
			player.Remove();
			
			var msg = string.Format(
				"'{0}' logged out. (Peer: {1}, E-Mail: '{2}', AccID: '{3}')",
				player.name,
				player.peer,
				player.email,
				player.account.id
			);
			
			// Log it
			LogManager.General.Log(msg);
			LogManager.Online.Log(msg);
		} else {
			var msg = string.Format(
				"Unknown player logged out, RemovePlayer has already been called. (E-Mail: '{0}', AccID: '{1}')",
				account.name,
				account.id.value
			);
			
			// Log it
			LogManager.General.LogWarning(msg);
			LogManager.Online.LogWarning(msg);
		}
	}
	
	// On application quit
	void OnApplicationQuit() {
		// Close all file handles
		LogManager.CloseAll();
	}
#endregion

#region RPCs
	[RPC]
	IEnumerator AccountLogIn(string email, string password, string deviceId, LobbyMessageInfo info) {
		// Check account activation
		bool activated = false;
		
		if(!GameDB.IsTestAccount(email)) {
			yield return LobbyGameDB.GetAccountAwaitingActivation(
				email,
				(data) => {
					if(string.IsNullOrEmpty(data))
						activated = true;
					else
						activated = false;
				}
			);
		} else {
			activated = true;
		}
		
		if(!activated) {
			Lobby.RPC("AccountNotActivated", info.sender, email);
			yield break;
		}
		
		// TODO: Check device ID
		
		// Login
		LogManager.General.Log("Login attempt: '" + email + "' on device " + deviceId);
		
		// Get account
		var getAccountReq = AccountManager.Master.TryGetAccount(email);
		yield return getAccountReq.WaitUntilDone();
		
		if(getAccountReq.isSuccessful) {
			var account = getAccountReq.result;
			
			// Log out account if logged in from a different peer
			if(account != null && AccountManager.Master.IsLoggedIn(account)) {
				var peer = AccountManager.Master.GetLoggedInPeer(account);
				
				LogManager.General.LogWarning(string.Format(
					"Account '{0}' already logged in, kicking old peer: {1}",
					account.name,
					peer
				));
				
				var logoutReq = AccountManager.Master.LogOut(peer);
				yield return logoutReq.WaitUntilDone();
			}
		}

		// Is the peer still connected?
		if(info.sender.type == LobbyPeerType.Disconnected) {
			LogManager.General.LogError("Peer disconnected, canceling login process: " + info.sender);
			yield break;
		}
		
		// Login
		var loginReq = AccountManager.Master.LogIn(info.sender, email, password);
		yield return loginReq.WaitUntilDone();
		
		if(!loginReq.isSuccessful) {
			var exception = (AccountException)loginReq.exception;
			var error = exception.error;
			
			// Bug in uLobby: We need to call this explicitly on the client
			Lobby.RPC("_RPCOnLogInFailed", info.sender, email, error);
			yield break;
		}
	}

	[RPC]
	void AccountLogOut(LobbyMessageInfo info) {
		AccountManager.Master.LogOut(info.sender);
	}

	[RPC]
	void RequestAccountInfo(string accountId, string propertyName, LobbyMessageInfo info) {
		var player = LobbyPlayer.Get(info);

		PlayerAccount.Get(accountId)[propertyName].GetObject((data) => {
			player.RPC("ReceiveAccountInfo", accountId, propertyName, data.GetType().FullName, Jboy.Json.WriteObject(data));
		});
	}
#endregion
}