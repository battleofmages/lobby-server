using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using uLobby;

public class LobbyPlayer : PartyMember<LobbyPlayer> {
	public static Dictionary<string, LobbyPlayer> accountIdToLobbyPlayer = new Dictionary<string, LobbyPlayer>();
	public static Dictionary<LobbyPeer, LobbyPlayer> peerToLobbyPlayer = new Dictionary<LobbyPeer, LobbyPlayer>();
	public static List<LobbyPlayer> list = new List<LobbyPlayer>();
	
	public Account account;
	public LobbyPeer peer;
	public CharacterCustomization custom;
	public ChatMember chatMember;
	public FriendsList friends;
	public PlayerStats stats;
	public PlayerStats ffaStats;
	public CharacterStats charStats;
	public ArtifactTree artifactTree;
	public ArtifactInventory artifactInventory;
	public ItemInventory itemInventory;
	public bool artifactsEditingFlag;
	public GuildList guildList;
	public List<string> guildInvitations;
	public AccessLevel accessLevel;
	public HashSet<LobbyPlayer> statusObservers;
	public HashSet<string> accountsWhereInfoIsRequired;
	public PlayerLocation _location;
	
	private string[] _followers;
	private OnlineStatus _onlineStatus;
	private LobbyParty _party;
	private HashSet<LobbyPlayer> playersReceivedStatus;
	
	public List<LobbyChatChannel> channels;
	
	private string _name;
	//private LobbyMatch _match;
	//private LobbyTown _town;
	private LobbyGameInstanceInterface _gameInstance;
	private LobbyQueue _queue;
	public LobbyGameInstanceInterface instanceAwaitingAccept;
	
	// Constructor
	public LobbyPlayer(Account nAccount) {
		account = nAccount;
		peer = AccountManager.Master.GetLoggedInPeer(account);
		
		statusObservers = new HashSet<LobbyPlayer>();
		statusObservers.Add(this);
		
		stats = null;
		ffaStats = null;
		custom = null;
		friends = null;
		_location = null;
		_followers = null;
		_party = new LobbyParty();
		_gameInstance = null;
		artifactsEditingFlag = false;
		channels = new List<LobbyChatChannel>();
		chatMember = new ChatMember(account.id.value);
		
		accountsWhereInfoIsRequired = new HashSet<string>();
		
		LobbyPlayer.list.Add(this);
		LobbyPlayer.accountIdToLobbyPlayer[account.id.value] = this;
		LobbyPlayer.peerToLobbyPlayer[peer] = this;
	}
	
	// Online status
	public OnlineStatus onlineStatus {
		get {
			return _onlineStatus;
		}
		
		set {
			_onlineStatus = value;
			BroadcastStatus();
		}
	}
	
	// Player name
	public string name {
		get {
			return _name;
		}
		
		set {
			_name = value;
		}
	}
	
	// Followers
	public string[] followers {
		get {
			return _followers;
		}
		
		set {
			_followers = value;
			BroadcastStatus();
		}
	}
	
	// Location
	public PlayerLocation location {
		get {
			return _location;
		}
		
		set {
			_location = value;
			
			// Save in database
			LocationsDB.SetLocation(accountId, _location, null);
			
			// Switch server
			switch(_location.serverType) {
				case ServerType.Town:
					ConnectToCurrentLocation<LobbyTown>((mapName) => {
						return new LobbyTown(mapName);
					});
					break;
					
				case ServerType.World:
					ConnectToCurrentLocation<LobbyWorld>((mapName) => {
						return new LobbyWorld(mapName);
					});
					break;
			}
		}
	}
	
	// SelectServer
	delegate T GameInstanceConstructor<T>(string mapName);
	void ConnectToCurrentLocation<T>(GameInstanceConstructor<T> constructor) where T : LobbyGameInstance<T> {
		// Start new server if needed
		T newInstance;
		
		List<LobbyGameInstance<T>> serverList;
		if(!LobbyGameInstance<T>.mapNameToInstances.TryGetValue(_location.mapName, out serverList) || serverList.Count == 0) {
			newInstance = constructor(_location.mapName);
			newInstance.Register();
		} else {
			var mapList = LobbyGameInstance<T>.mapNameToInstances[_location.mapName];
			var mapIndex = Random.Range(0, mapList.Count - 1);
			var lobbyGameInstance = mapList[mapIndex];
			newInstance = (T)lobbyGameInstance;
		}
		
		// Connect the player once the instance is ready
		LobbyServer.instance.StartCoroutine(ConnectToGameInstanceDelayed(newInstance));
	}
	
	// OnFriendsListLoaded
	public void OnFriendsListLoaded() {
		// Send new friends list
		Lobby.RPC("ReceiveFriendsList", peer, accountId, Jboy.Json.WriteObject(friends));
		
		// TODO: Only send changes
		foreach(var group in friends.groups) {
			foreach(var friend in group.friends) {
				accountsWhereInfoIsRequired.Add(friend.accountId);
			}
		}
		
		// Send player info for all the accounts on the friends list
		SendInfoAboutOtherAccounts();
	}
	
	// OnFollowersListLoaded
	public void OnFollowersListLoaded() {
		// Send new friends list
		Lobby.RPC("ReceiveFollowersList", peer, accountId, followers, true);
		
		// Combine account lists
		accountsWhereInfoIsRequired.UnionWith(followers);
		
		// Send player info for all the accounts on the followers list
		SendInfoAboutOtherAccounts();
	}
	
	// SendInfoAboutOtherAccounts
	void SendInfoAboutOtherAccounts() {
		if(friends == null || followers == null)
			return;
		
		foreach(var friendAccountId in accountsWhereInfoIsRequired) {
			string friendName = null;
			
			// Send name
			if(GameDB.accountIdToName.TryGetValue(friendAccountId, out friendName)) {
				Lobby.RPC("ReceivePlayerName", peer, friendAccountId, friendName);
			} else {
				string lambdaAccountId = friendAccountId;
				
				LobbyGameDB.GetPlayerName(lambdaAccountId, data => {
					if(data != null) {
						GameDB.accountIdToName[lambdaAccountId] = data;
						Lobby.RPC("ReceivePlayerName", this.peer, lambdaAccountId, data);
					}
				});
			}
			
			// Send initial online status
			LobbyPlayer player;
			if(accountIdToLobbyPlayer.TryGetValue(friendAccountId, out player))
				Lobby.RPC("ReceiveOnlineStatus", this.peer, friendAccountId, player.onlineStatus);
		}
	}
	
	// Clean up on leaving the instance
	private void OnLeaveInstance() {
		if(_gameInstance == null)
			return;
		
		// Remove from player list
		_gameInstance.players.Remove(this);
		
		// Leave map chat channel
		if(_gameInstance.mapChannel != null)
			_gameInstance.mapChannel.RemovePlayer(this);
	}
	
	// Game instance
	public LobbyGameInstanceInterface gameInstance {
		get {
			return _gameInstance;
		}
		
		set {
			// Value is valid
			if(value != null) {
				if(value != _gameInstance) {
					OnLeaveInstance();
					
					_gameInstance = value;
					_gameInstance.players.Add(this);
				}
				
				if(_gameInstance.mapChannel != null && !channels.Contains(_gameInstance.mapChannel))
					_gameInstance.mapChannel.AddPlayer(this);
				
				if(inMatch || inFFA)
					onlineStatus = OnlineStatus.InMatch;
			// Value is null
			} else {
				OnLeaveInstance();
				
				onlineStatus = OnlineStatus.Online;
				_gameInstance = value;
			}
			
			BroadcastStatus();
		}
	}
	
	// In match
	public bool inMatch {
		get {
			return _gameInstance is LobbyMatch;
		}
	}
	
	// In town
	public bool inTown {
		get {
			return _gameInstance is LobbyTown;
		}
	}
	
	// In world
	public bool inWorld {
		get {
			return _gameInstance is LobbyWorld;
		}
	}
	
	// In FFA
	public bool inFFA {
		get {
			return _gameInstance is LobbyFFA;
		}
	}
	
	// In PvP
	public bool inPvP {
		get {
			return inMatch || inFFA;
		}
	}
	
	// In dungeon
	public bool inDungeon {
		get {
			return false;
		}
	}
	
	// Can enter PvP
	public bool canEnterPvP {
		get {
			return !inPvP && !inDungeon;
		}
	}
	
	// Match
	public LobbyMatch match {
		get {
			return _gameInstance as LobbyMatch;
		}
	}
	
	// Town
	public LobbyTown town {
		get {
			return _gameInstance as LobbyTown;
		}
	}
	
	// FFA
	public LobbyFFA ffa {
		get {
			return _gameInstance as LobbyFFA;
		}
	}
	
	// World
	public LobbyWorld world {
		get {
			return _gameInstance as LobbyWorld;
		}
	}
	
	// Instance
	public uZone.InstanceProcess instance {
		get {
			if(inMatch)
				return match.instance;
			
			if(inTown)
				return town.instance;
			
			if(inFFA)
				return ffa.instance;
			
			if(inWorld)
				return world.instance;
			
			return null;
		}
	}
	
	// Queue
	public LobbyQueue queue {
		get {
			return _queue;
		}
		
		set {
			_queue = value;
			
			if(_queue != null)
				onlineStatus = OnlineStatus.InQueue;
			else
				onlineStatus = OnlineStatus.Online;
			
			BroadcastStatus();
		}
	}
	
	// Account ID
	public string accountId {
		get {
			return account.id.value;
		}
	}
	
	// IP
	public string ip {
		get {
			return peer.endpoint.Address.ToString();
		}
	}
	
	// Disconnected
	public bool disconnected {
		get {
			if(peer == null)
				return true;
			
			return peer.type == LobbyPeerType.Disconnected;
		}
	}
	
	// Can use map chat
	public bool canUseMapChat {
		get {
			return _gameInstance != null;
		}
	}
	
	// Removes a player - This function can be called from logout, disconnect and SendQueueStats!
	public void Remove() {
		// Remove the player from the queue he was in
		if(queue != null)
			queue.RemovePlayer(this);
		
		// Remove game instance associations
		gameInstance = null;
		
		// Broadcast offline status
		onlineStatus = OnlineStatus.Offline;
		
		// Remove the reference from the dictionary
		LobbyPlayer.accountIdToLobbyPlayer.Remove(accountId);
		
		// Remove the player from the global list
		LobbyPlayer.list.Remove(this);
		
		// TODO: When we save parties, this shouldn't exist
		// Leave party
		var pty = GetParty();
		if(pty != null)
			pty.RemoveMember(this);
		
		// Remove the player from all chat channels.
		// The list is copied because channels could be deleted after removing players.
		foreach(var channel in new List<LobbyChatChannel>(channels)) {
			channel.RemovePlayer(this);
		}
		
		// Treat him as if he is disconnected for existing objects
		peer = null;
	}
	
	// Returns a player to his world location
	public void ReturnToWorld() {
		// Map name
		string playerMap = MapManager.defaultTown;
		
		// Start new town server if needed
		LobbyTown townInstance;
		List<LobbyGameInstance<LobbyTown>> townList;
		if(!LobbyTown.mapNameToInstances.TryGetValue(playerMap, out townList) || townList.Count == 0) {
			townInstance = new LobbyTown(playerMap);
			townInstance.Register();
		} else {
			var lobbyGameInstance = LobbyTown.mapNameToInstances[playerMap][0];
			townInstance = (LobbyTown)lobbyGameInstance;
		}
		
		// Connect the player once the instance is ready
		LobbyServer.instance.StartCoroutine(ConnectToGameInstanceDelayed(townInstance));
	}
	
	// Broadcast status
	void BroadcastStatus() {
		playersReceivedStatus = new HashSet<LobbyPlayer>();
		
		// Status observers
		BroadcastStatus(statusObservers);
		
		// Friends
		if(friends != null)
			BroadcastStatus(from friend in friends.allFriends select friend.accountId);
		
		// Followers
		if(followers != null)
			BroadcastStatus(followers);
	}
	
	// Broadcast status to a group of players
	void BroadcastStatus(IEnumerable<LobbyPlayer> players) {
		foreach(var player in players) {
			if(!Lobby.IsPeerConnected(player.peer))
				continue;
			
			if(!playersReceivedStatus.Contains(player)) {
				Lobby.RPC("ReceiveOnlineStatus", player.peer, this.accountId, _onlineStatus);
				playersReceivedStatus.Add(player);
			}
		}
	}
	
	// Broadcast status to a group of account IDs
	void BroadcastStatus(IEnumerable<string> accountIds) {
		LobbyPlayer player;
		foreach(var accountId in accountIds) {
			if(!accountIdToLobbyPlayer.TryGetValue(accountId, out player))
				continue;
			
			if(!Lobby.IsPeerConnected(player.peer))
				continue;
			
			if(!playersReceivedStatus.Contains(player)) {
				Lobby.RPC("ReceiveOnlineStatus", player.peer, this.accountId, _onlineStatus);
				playersReceivedStatus.Add(player);
			}
		}
	}
	
	// Send match accept request
	public void SendMatchAcceptRequest(LobbyMatch newMatch) {
		instanceAwaitingAccept = newMatch;
		Lobby.RPC("MatchFound", this.peer);
	}
	
	// Accept match
	public void AcceptMatch() {
		var acceptedMatch = this.instanceAwaitingAccept as LobbyMatch;
		
		// This should never happen
		if(acceptedMatch == null) {
			LogManager.General.LogError("Accepted instance is not a match: " + acceptedMatch.ToString());
			return;
		}
		
		this.instanceAwaitingAccept = null;
		acceptedMatch.UpdatePlayerAccept();
	}
	
	// Deny match
	public void DenyMatch() {
		var deniedMatch = this.instanceAwaitingAccept as LobbyMatch;
		
		// This should never happen
		if(deniedMatch == null) {
			LogManager.General.LogError("Denied instance is not a match: " + deniedMatch.ToString());
			return;
		}
		
		deniedMatch.Cancel();
	}
	
	// Makes the player leave the queue
	public bool LeaveQueue() {
		if(queue == null)
			return false;
		
		queue.RemovePlayer(this);
		queue = null;
		
		return true;
	}
	
	// Connects the player to a game server instance, delayed
	public IEnumerator ConnectToGameInstanceDelayed<T>(LobbyGameInstance<T> lobbyGameInstance) {
		// TODO: Add a timeout
		
		// Wait for instance to be online
		while(lobbyGameInstance.instance == null && !disconnected) {
			yield return new WaitForSeconds(0.01f);
		}
		
		// Still null, or player disconnected?
		if(lobbyGameInstance.instance == null || disconnected)
			yield break;
		
		// Connect player to server
		ConnectToGameInstance(lobbyGameInstance);
	}
	
	// Connects the player to a game server instance
	public void ConnectToGameInstance<T>(LobbyGameInstance<T> lobbyGameInstance) {
		gameInstance = lobbyGameInstance;
		
		ConnectToGameServer(lobbyGameInstance.instance);
	}
	
	// Helper function
	private void ConnectToGameServer(uZone.InstanceProcess instance) {
		LogManager.General.Log("Connecting player '" + name + "' to " + gameInstance);
		
		var ip = instance.node.publicAddress;
		var port = instance.port;
		
		// Connect player to server
		Lobby.RPC("ConnectToGameServer", peer, ip, port);
		
		// Update location
		location.ip = ip;
		location.port = port;
		LocationsDB.SetLocation(accountId, location, null);
	}
	
	// UpdateCountry
	public void UpdateCountry() {
		if(IPInfoServer.ipToCountry.ContainsKey(ip)) {
			IPInfoDB.SetCountry(
				accountId,
				IPInfoServer.ipToCountry[ip],
				data => {
					if(data != null) {
						IPInfoServer.accountIdToCountry[accountId] = data;
					}
				}
			);
		}
	}
	
	// Account is online
	public static bool AccountIsOnline(string checkAccountId) {
		return accountIdToLobbyPlayer.ContainsKey(checkAccountId);
	}
	
	// SetParty
	public void SetParty(Party<LobbyPlayer> pty) {
		_party = (LobbyParty)pty;
	}
	
	// GetParty
	public Party<LobbyPlayer> GetParty() {
		return _party;
	}
	
	// GetAccountId
	public string GetAccountId() {
		return accountId;
	}
	
	// ToString
	public override string ToString() {
		return _name;
	}
}