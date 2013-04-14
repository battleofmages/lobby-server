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
	public ChatMember chatMember;
	public PlayerStats stats;
	public CharacterStats charStats;
	public List<string> guildIdList;
	public List<string> guildInvitations;
	
	public List<LobbyChatChannel> channels;
	
	private string _name;
	private bool _inMatch;
	private LobbyQueue _queue;
	
	// Constructor
	public LobbyPlayer(Account nAccount) {
		account = nAccount;
		peer = AccountManager.Master.GetLoggedInPeer(account);
		stats = null;
		channels = new List<LobbyChatChannel>();
		chatMember = new ChatMember(_name, ChatMemberStatus.Online);
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
			return _inMatch;
		}
		
		set {
			_inMatch = value;
			
			if(_inMatch)
				this.chatMember.status = ChatMemberStatus.InMatch;
			else
				this.chatMember.status = ChatMemberStatus.Online;
			
			this.BroadcastStatus();
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
	
	// Update the status
	void BroadcastStatus() {
		foreach(var channel in this.channels) {
			channel.Broadcast(p => Lobby.RPC("ChatStatus", p.peer, channel.name, this.chatMember));
		}
	}
	
	// Makes the player leave the queue
	public void LeaveQueue() {
		if(queue != null) {
			queue.RemovePlayer(this);
			queue = null;
		}
	}
	
	// Connects the player to a game server instance
	public void ConnectToGameServer(uZone.GameInstance instance) {
		XDebug.Log("Connecting account '" + account.name + "' to game server " + instance.ip + ":" + instance.port);
		Lobby.RPC("ConnectToGameServer", peer, instance.ip, instance.port);
	}
}