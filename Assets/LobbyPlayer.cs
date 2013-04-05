using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

/*public class LobbyParty {
	public LobbyPlayer[] players;
}*/

public class LobbyPlayer {
	public static Dictionary<Account, LobbyPlayer> accountToLobbyPlayer = new Dictionary<Account, LobbyPlayer>();
	public static List<LobbyPlayer> list = new List<LobbyPlayer>();
	
	public Account account;
	public LobbyPeer peer;
	public string name;
	public PlayerStats stats;
	public CharacterStats charStats;
	public LobbyQueue queue;
	public bool inMatch;
	public List<LobbyChatChannel> channels;
	
	// Constructor
	public LobbyPlayer(Account nAccount) {
		account = nAccount;
		peer = AccountManager.Master.GetLoggedInPeer(account);
		queue = null;
		stats = null;
		channels = new List<LobbyChatChannel>();
		LobbyPlayer.list.Add(this);
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
		Debug.Log("Connecting account '" + account.name + "' to game server " + instance.ip + ":" + instance.port);
		Lobby.RPC("ConnectToGameServer", peer, instance.ip, instance.port);
	}
}