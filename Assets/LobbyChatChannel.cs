using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;
using System.Linq;

public class LobbyChatChannel {
	public static Dictionary<string, LobbyChatChannel> channels = new Dictionary<string, LobbyChatChannel>();
	
	public List<LobbyPlayer> members;
	public string name;
	
	public LobbyChatChannel(string channelName) {
		members = new List<LobbyPlayer>();
		name = channelName;
		channels[channelName] = this;
	}
	
	public void Unregister() {
		foreach(var member in members) {
			this.RemovePlayer(member);
		}
		
		if(channels.ContainsKey(name))
			LobbyChatChannel.channels.Remove(name);
	}
	
	public bool isGameChannel {
		get { return name.IndexOf('@') != -1; }
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
			//if(player.inMatch)
			//	continue;
			
			if(!Lobby.IsPeerConnected(player.peer))
				continue;
			
			func(player);
			//Lobby.RPC("Chat", player.peer, this.name, args);
		}
	}
	
	public void AddPlayer(LobbyPlayer player) {
		this.SendMemberListToPlayer(player);
		
		members.Add(player);
		player.channels.Add(this);
		
		this.Broadcast(p => Lobby.RPC("ChatJoin", p.peer, this.name, player.chatMember));
	}
	
	public void RemovePlayer(LobbyPlayer player) {
		members.Remove(player);
		player.channels.Remove(this);
		
		this.Broadcast(p => Lobby.RPC("ChatLeave", p.peer, this.name, player.chatMember.name));
		
		// TODO: If someone connects first and instantly disconnects, this would be a bug
		/*if(isGameChannel && members.Count == 0) {
			this.Unregister();
		}*/
	}
	
	public void SendMemberListToPlayer(LobbyPlayer player) {
		var chatMemberList = this.members.Select(o => o.chatMember).ToArray();
		//Debug.Log ("Sending " + chatMemberList + " list with " + chatMemberList.Length + " entries");
		
		// Receive member list
		Lobby.RPC("ChatMembers", player.peer, this.name, chatMemberList);
	}
}
