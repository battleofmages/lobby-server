using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LobbyQueue {
	public static int maxRankingRange = 99;
	
	private List<LobbyPlayer> playerList;
	
	private int _unitsNeededForGameStart;
	private int _playersPerTeam;
	
	// Units needed for game start
	public int unitsNeededForGameStart {
		get {
			return _unitsNeededForGameStart;
		}
		
		set {
			_unitsNeededForGameStart = value;
			_playersPerTeam = _unitsNeededForGameStart / 2;
		}
	}
	
	// Constructor
	public LobbyQueue() {
		playerList = new List<LobbyPlayer>();
	}
	
	// Player count
	public int playerCount {
		get { return playerList.Count; }
	}
	
	// Add player
	public bool AddPlayer(LobbyPlayer player) {
		// Remove player from his old queue if he is still in
		if(player.queue != null) {
			if(player.queue == this)
				return false;
			
			player.queue.RemovePlayer(player);
		}
		
		// Add him to this queue
		playerList.Add(player);
		
		// Set this queue to this one
		player.queue = this;
		
		return true;
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
				CreateMatchInRange(i, unitsNeededForGameStart);
				
				// Register in the waiting list
				//match.Register();
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
			CreateMatchInRange(0, unitsNeededForGameStart);
			
			// Register in the waiting list
			//match.Register();
		}
	}
	
	// Creates a match for players in the list starting at the given index
	LobbyMatch CreateMatchInRange(int start, int length) {
		LobbyMatch match = new LobbyMatch();
		LobbyPlayer player;
		
		// TODO: Atm we take the first X players from the queue
		int end = start + length;
		for(int i = start; i < end; i++) {
			// TODO: Optimize algorithm
			while(true) {
				player = playerList[Random.Range(start, end)];
				
				if(player.queue != null) {
					break;
				}
			}
			
			// Set player queue to null, WE REMOVE THEM LATER, all at once
			player.queue = null;
			
			//player.match = match;
			player.SendMatchAcceptRequest(match);
			match.teams[i / _playersPerTeam].Add(player);
		}
		
		// Remove those players from the queue
		playerList.RemoveRange(start, length);
		
		return match;
	}
	
	// Remove player
	public void RemovePlayer(LobbyPlayer player) {
		playerList.Remove(player);
		player.queue = null;
	}
	
	// Create practice match
	public static LobbyMatch CreatePracticeMatch(LobbyPlayer player) {
		player.LeaveQueue();
		
		LobbyMatch match = new LobbyMatch();
		
		//player.match = match;
		match.teams[0].Add(player);
		player.SendMatchAcceptRequest(match);
		
		return match;
	}
}
