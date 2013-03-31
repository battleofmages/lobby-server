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
	
	public int maxConnections = 1024;
	public int listenPort = 1310;
	public string databaseHost = "blitzprog.org";
	public int databasePort = 8087;
	public string uZoneHost = "127.0.0.1";
	public int uZonePort = 12345;
	
	private LobbyQueue[] queue;
	
	void Awake() {
		// TODO: Set this to 1 or 0 on release, we don't need Update() or OnGUI()
		// Limit frame rate
		//Application.targetFrameRate = 25;
		uLinkTargetFrameRateFix.SetTargetFrameRate(25);
		
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
		// Make this class listen to Lobby events
		Lobby.AddListener(this);
		
		// Initialize the lobby
		Debug.Log("Initializing lobby on port " + listenPort + " with a maximum of " + maxConnections + " players.");
		Lobby.InitializeLobby(maxConnections, listenPort, databaseHost, databasePort);
		
		// Add ourselves as listeners for when accounts log in or out.
		AccountManager.OnAccountLoggedIn += OnAccountLoggedIn;
		AccountManager.OnAccountLoggedOut += OnAccountLoggedOut;
		
		// Send queue stats
		InvokeRepeating("SendQueueStats", 1.0f, 1.0f);
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
		Debug.LogWarning ("uZone error code: " + error);
	}
	
	// Lobby initialized
	void uLobby_OnLobbyInitialized() {
		Debug.Log("Successfully initialized lobby.");
	}
	
	// Account login
	void OnAccountLoggedIn(Account account) {
		Debug.Log("Account '" + account.name + "' logged in.");
		
		// Save the reference in a dictionary
		LobbyPlayer lobbyPlayer = new LobbyPlayer(account);
		LobbyPlayer.accountToLobbyPlayer[account] = lobbyPlayer;
		
		// Async: Retrieve the player name
		StartCoroutine(LobbyGameDB.GetPlayerName(lobbyPlayer));
		StartCoroutine(LobbyGameDB.GetPlayerStats(lobbyPlayer));
	}
	
	// Account logout
	void OnAccountLoggedOut(Account account) {
		//Debug.Log("'" + account.name + "' logged out.");
		
		LobbyPlayer player = LobbyPlayer.accountToLobbyPlayer[account];
		RemovePlayer(player);
	}
	
	void RemovePlayer(LobbyPlayer player) {
		// Remove the player from the queue he was in
		if(player.queue != null)
			player.queue.RemovePlayer(player);
		
		// Remove the reference from the dictionary
		LobbyPlayer.accountToLobbyPlayer.Remove(player.account);
		
		// Remove the player from the global list
		LobbyPlayer.list.Remove(player);
		
		// Log it
		Debug.Log("Player '" + player.name + "' from account '" + player.account.name + "' logged out.");
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
				Debug.Log("Couldn't send queue data to player '" + player.name + "' from account '" + player.account.name + "'");
			}
		}
		
		// Clear offline players
		foreach(LobbyPlayer player in offlinePlayers) {
			RemovePlayer(player);
		}
	}
	
	LobbyPlayer GetLobbyPlayer(LobbyMessageInfo info) {
		Account account = AccountManager.Master.GetLoggedInAccount(info.sender);
		return LobbyPlayer.accountToLobbyPlayer[account];
	}
	
	[RPC]
	void PlayerNameChange(string newName, LobbyMessageInfo info) {
		// Length minimum
		if(newName.Length < 2)
			return;
		
		// Get the account from the uLobby.LobbyPeer
		Account acc = AccountManager.Master.GetLoggedInAccount(info.sender);
		
		// Change name
		Debug.Log(acc.name + " has requested to change his player name to '" + newName + "'");
		StartCoroutine(LobbyGameDB.SetPlayerName(acc, newName));
	}
	
	[RPC]
	void EnterQueue(byte playersPerTeam, LobbyMessageInfo info) {
		// Check for correct team size
		if(playersPerTeam > 5)
			return;
		
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		// Add the player to the queue
		queue[playersPerTeam - 1].AddPlayer(lobbyPlayer);
		
		// Let the player know he entered the queue
		Debug.Log("Added '" + lobbyPlayer.name + "' to " + playersPerTeam + "v" + playersPerTeam + " queue");
		Lobby.RPC("EnteredQueue", lobbyPlayer.peer, playersPerTeam);
	}
	
	[RPC]
	void LeaveQueue(LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		// Make the player leave the queue
		lobbyPlayer.LeaveQueue();
		
		// Let the player know he left the queue
		Debug.Log("'" + lobbyPlayer.name + "' left the queue");
		Lobby.RPC("LeftQueue", lobbyPlayer.peer);
	}
	
	[RPC]
	void ReturnedFromMatch(LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = GetLobbyPlayer(info);
		
		// A player just returned from a match, we'll send him queue infos again
		lobbyPlayer.inMatch = false;
		
		// Send him his new stats
		StartCoroutine(LobbyGameDB.GetPlayerStats(lobbyPlayer));
	}
}
