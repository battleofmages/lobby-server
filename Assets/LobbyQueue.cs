using uLobby;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LobbyQueue {
	public static int maxRankingRange = 99;
	
	private List<LobbyPlayer> playerList;
	
	private int _unitsNeededForGameStart;
	private int _playersPerTeam;
	
	public int unitsNeededForGameStart {
		get {
			return _unitsNeededForGameStart;
		}
		
		set {
			_unitsNeededForGameStart = value;
			_playersPerTeam = _unitsNeededForGameStart / 2;
		}
	}
	
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
		MakeMatchesBasedOnRanking();
	}
	
	// With ranking in mind
	public void MakeMatchesBasedOnRanking() {
		// Not enough players?
		if(playerList.Count < unitsNeededForGameStart)
			return;
			
		// Sort the list
		// TODO: Insertion sort
		playerList = playerList.OrderBy(o => o.stats.bestRanking).ToList();
		
		int i = 0;
		
		while(i <= playerList.Count - unitsNeededForGameStart) {
			LobbyPlayer lowestPlayer = playerList[i];
			LobbyPlayer highestPlayer = playerList[i + (unitsNeededForGameStart - 1)];
			
			// Ranking difference should be lower than maxRankingRange
			if(highestPlayer.stats.bestRanking - lowestPlayer.stats.bestRanking <= maxRankingRange) {
				// Create a match
				var match = CreateMatchInRange(i, unitsNeededForGameStart);
				
				// Register in the waiting list
				match.Register();
			} else {
				// Skip to next player
				i += 1;
			}
		}
	}
	
	// Without ranking
	public void MakeMatches() {
		while(playerList.Count >= unitsNeededForGameStart) {
			// Create a match
			var match = CreateMatchInRange(0, unitsNeededForGameStart);
			
			// Register in the waiting list
			match.Register();
		}
	}
	
	// Creates a match for players in the list starting at the given index
	Match CreateMatchInRange(int start, int length) {
		Match match = new Match();
		
		// TODO: Atm we take the first X players from the queue
		int end = start + length;
		for(int i = start; i < end; i++) {
			LobbyPlayer player = playerList[i];
			
			// Set player queue to null, WE REMOVE THEM LATER, all at once
			player.queue = null;
			
			player.inMatch = true;
			Lobby.RPC("MatchFound", player.peer);
			match.teams[i / _playersPerTeam].Add(player);
		}
		
		// Remove those players from the queue
		playerList.RemoveRange(start, length);
		
		return match;
	}
	
	public void RemovePlayer(LobbyPlayer player) {
		playerList.Remove(player);
		player.queue = null;
	}
}
