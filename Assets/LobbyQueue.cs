using uLobby;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LobbyQueue {
	private List<LobbyPlayer> playerList;
	public int unitsNeededForGameStart;
	
	public LobbyQueue() {
		playerList = new List<LobbyPlayer>();
	}
	
	public int playerCount {
		get { return playerList.Count; }
	}
	
	public void AddPlayer(LobbyPlayer player) {
		// Remove player from his old queue if he is still in
		if(player.queue != null)
			player.queue.RemovePlayer(player);
		
		// Add him to this queue
		playerList.Add(player);
		
		// Set this queue to this one
		player.queue = this;
		
		// Make matches since new players joined
		MakeMatches();
	}
	
	public void MakeMatches() {
		// TODO: Cache this
		int playersPerTeam = unitsNeededForGameStart / 2;
		
		while(playerList.Count >= unitsNeededForGameStart) {
			Match match = new Match();
			
			// TODO: Atm we take the first X players from the queue
			for(int i = 0; i < unitsNeededForGameStart; i++) {
				LobbyPlayer player = playerList[i];
				
				// Set player queue to null, WE REMOVE THEM LATER, all at once
				player.queue = null;
				
				player.inMatch = true;
				Lobby.RPC("MatchFound", player.peer);
				match.teams[i / playersPerTeam].Add(player);
			}
			
			// Remove those players from the queue
			playerList.RemoveRange(0, unitsNeededForGameStart);
			
			// Register in the waiting list
			match.Register();
		}
	}
	
	public void RemovePlayer(LobbyPlayer player) {
		playerList.Remove(player);
		player.queue = null;
	}
}
