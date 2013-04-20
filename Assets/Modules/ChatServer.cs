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
	bool ProcessLobbyChatCommands(LobbyPlayer lobbyPlayer, string msg) {
		if(msg.StartsWith("//ginvite ")) {
			/*StartCoroutine(lobbyGameDB.GetAccountIdByPlayerName(msg.Split(' ')[1], data => {
				XDebug.Log ("ginvite: " + data);
			}));*/
			return true;
		}
		
		return false;
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void ClientChat(string channelName, string msg, LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = LobbyServer.GetLobbyPlayer(info);
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
}
