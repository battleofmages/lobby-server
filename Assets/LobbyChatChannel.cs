using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;
using System.Linq;

public class LobbyChatChannel {
	public static Dictionary<string, LobbyChatChannel> channels = new Dictionary<string, LobbyChatChannel>();
	
	public List<LobbyPlayer> members;
	protected string name;
	
	public LobbyChatChannel(string channelName) {
		members = new List<LobbyPlayer>();
		name = channelName;
		channels.Add(channelName, this);
	}
	
	public void BroadcastMessage(string msg) {
		this.Broadcast(p => Lobby.RPC("Chat", p.peer, this.name, "", msg));
	}
	
	public void BroadcastMessage(string playerName, string msg) {
		this.Broadcast(p => Lobby.RPC("Chat", p.peer, this.name, playerName, msg));
	}
	
	// Delegate type
	public delegate void ActionPerPlayer(LobbyPlayer player);
	
	// Calls a function on each player in the channel
	public void Broadcast(ActionPerPlayer func) {
		foreach(var player in members) {
			if(player.inMatch)
				continue;
			
			if(!Lobby.IsPeerConnected(player.peer))
				continue;
			
			func(player);
			//Lobby.RPC("Chat", player.peer, this.name, args);
		}
	}
	
	public void AddPlayer(LobbyPlayer player) {
		// Receive member list
		Lobby.RPC("ChatMembers", player.peer, this.name, this.members.Select(o => o.name).ToArray());
		
		members.Add(player);
		player.channels.Add(this);
		
		this.Broadcast(p => Lobby.RPC("ChatJoin", p.peer, this.name, player.name));
	}
	
	public void RemovePlayer(LobbyPlayer player) {
		members.Remove(player);
		player.channels.Remove(this);
		
		this.Broadcast(p => Lobby.RPC("ChatLeave", p.peer, this.name, player.name));
	}
}
