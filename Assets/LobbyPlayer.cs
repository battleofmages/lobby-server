using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

/*public class LobbyParty {
	public LobbyPlayer[] players;
}*/

public class LobbyPlayer {
	public static Dictionary<string, LobbyPlayer> accountIdToLobbyPlayer = new Dictionary<string, LobbyPlayer>();
	public static List<LobbyPlayer> list = new List<LobbyPlayer>();
	
	public Account account;
	public LobbyPeer peer;
	public CharacterCustomization custom;
	public ChatMember chatMember;
	public PlayerStats stats;
	public CharacterStats charStats;
	public ArtifactTree artifactTree;
	public ArtifactInventory artifactInventory;
	public bool artifactsEditingFlag;
	public GuildList guildList;
	public List<string> guildInvitations;
	public AccessLevel accessLevel;
	
	public List<LobbyChatChannel> channels;
	
	private string _name;
	private LobbyMatch _match;
	private LobbyTown _town;
	private LobbyQueue _queue;
	
	// Constructor
	public LobbyPlayer(Account nAccount) {
		account = nAccount;
		peer = AccountManager.Master.GetLoggedInPeer(account);
		stats = null;
		custom = null;
		_match = null;
		artifactsEditingFlag = false;
		channels = new List<LobbyChatChannel>();
		chatMember = new ChatMember(_name, ChatMemberStatus.Online);
		//artifactInventories = new Inventory();
		
		LobbyPlayer.list.Add(this);
		LobbyPlayer.accountIdToLobbyPlayer[account.id.value] = this;
	}
	
	// Player name
	public string name {
		get {
			return _name;
		}
		
		set {
			_name = value;
			chatMember.name = value;
		}
	}
	
	// In match
	public bool inMatch {
		get {
			return _match != null;
		}
	}
	
	// In town
	public bool inTown {
		get {
			return _town != null;
		}
	}
	
	public LobbyMatch match {
		get {
			return _match;
		}
		
		set {
			if(value != null) {
				this.chatMember.status = ChatMemberStatus.InMatch;
			} else {
				this.chatMember.status = ChatMemberStatus.Online;
				
				// Leave game chat channel
				if(_match != null) {
					_match.mapChannel.RemovePlayer(this);
				}
			}
			
			_match = value;
			this.BroadcastStatus();
		}
	}
	
	public LobbyTown town {
		get {
			return _town;
		}
		
		set {
			// TODO: ...
			_town = value;
		}
	}
	
	public uZone.GameInstance instance {
		get {
			if(_match != null)
				return _match.instance;
			
			if(_town != null)
				return _town.instance;
			
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
				this.chatMember.status = ChatMemberStatus.InQueue;
			else
				this.chatMember.status = ChatMemberStatus.Online;
			
			this.BroadcastStatus();
		}
	}
	
	// Account ID
	public string accountId {
		get {
			return account.id.value;
		}
	}
	
	// Update the status
	void BroadcastStatus() {
		foreach(var channel in this.channels) {
			channel.Broadcast(p => Lobby.RPC("ChatStatus", p.peer, channel.name, this.chatMember));
		}
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
		while(lobbyGameInstance.instance == null && this.peer.type != LobbyPeerType.Disconnected) {
			System.Threading.Thread.Sleep(50);
			yield return null;
		}
		
		// Player disconnected?
		if(lobbyGameInstance.instance == null)
			yield break;
		
		// Connect player to server
		this.ConnectToGameInstance(lobbyGameInstance);
	}
	
	// Connects the player to a game server instance
	public void ConnectToGameInstance<T>(LobbyGameInstance<T> lobbyGameInstance) {
		this.match = lobbyGameInstance as LobbyMatch;
		this.town = lobbyGameInstance as LobbyTown;
		
		this.ConnectToGameServer(lobbyGameInstance.instance);
		lobbyGameInstance.mapChannel.AddPlayer(this);
	}
	
	// Helper function
	private void ConnectToGameServer(uZone.GameInstance instance) {
		LogManager.General.Log("Connecting account '" + account.name + "' to " + (this.inTown ? "Town" : "Arena") + " server " + instance.ip + ":" + instance.port);
		Lobby.RPC("ConnectToGameServer", peer, instance.ip, instance.port);
	}
	
	// Account is online
	public static bool AccountIsOnline(string checkAccountId) {
		return accountIdToLobbyPlayer.ContainsKey(checkAccountId);
	}
}