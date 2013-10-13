using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class ChatServer : MonoBehaviour {
	void Start () {
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// User and admin commands
	bool ProcessLobbyChatCommands(LobbyPlayer player, string msg) {
		switch(msg) {
		case "//practice":
			if(!player.inMatch) {
				var match = LobbyQueue.CreatePracticeMatch(player);
				match.Register();
			} else {
				// Notify player ...
			}
			return true;
			
		default:
			if(msg.StartsWith("//ginvite ")) {
				/*StartCoroutine(lobbyGameDB.GetAccountIdByPlayerName(msg.Split(' ')[1], data => {
					Debug.Log ("ginvite: " + data);
				}));*/
				return true;
			}
			
			return false;
		}
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void ClientChat(string channelName, string msg, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		// Command?
		if(ProcessLobbyChatCommands(player, msg)) {
			LogManager.Chat.Log("[" + channelName + "][" + player.name + "] '" + msg + "'");
			return;
		}
		
		// Add instance to channel name
		if(channelName == "Map" && (player.inMatch || player.inTown)) {
			var instance = player.instance;
			
			if(instance == null) {
				LogManager.Chat.Log("[" + channelName + "][" + player.name + "] '" + msg + "'");
				return;
			}
			
			var postfix = instance.ip + ":" + instance.port;
			channelName += "@" + postfix;
		}
		
		// Log all chat tries
		LogManager.Chat.Log("[" + channelName + "][" + player.name + "] '" + msg + "'");
		
		// Access level?
		if(channelName == "Announcement" && player.accessLevel < AccessLevel.CommunityManager)
			return;
		
		// Does the channel exist?
		if(!LobbyChatChannel.channels.ContainsKey(channelName))
			return;
		
		var channel = LobbyChatChannel.channels[channelName];
		
		// Channel member?
		if(!channel.members.Contains(player))
			return;
		
		// Broadcast message
		channel.BroadcastMessage(player.name, msg);
	}
}
