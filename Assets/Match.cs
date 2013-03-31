using uLobby;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Match {
	public static List<Match> matchesWaitingForServer = new List<Match>();
	public static List<Match> matchesRunning = new List<Match>();
	
	public List<LobbyPlayer>[] teams;
	public uZone.GameInstance instance;
	public int requestId;
	
	public Match() {
		// Create the player lists
		teams = new List<LobbyPlayer>[4];
		
		for(int i = 0; i < teams.Length; i++) {
			teams[i] = new List<LobbyPlayer>();
		}
		
		instance = null;
	}
	
	// Registers this match
	public void Register() {
		Match.matchesWaitingForServer.Add(this);
		
		// Prepare arguments: -team0 account1 account2 -team1 account3 account4
		List<string> args = new List<string>();
		
		for(int i = 0; i < teams.Length; i++) {
			args.Add("-party" + i);
			
			foreach(LobbyPlayer player in teams[i]) {
				args.Add(player.account.id.value);
			}
		}
		
		// Async: Start game server instance for this match
		requestId = uZone.InstanceManager.StartGameInstance(LobbyServer.gameName, args);
	}
	
	// Starts playing on game server instance
	public void StartPlayingOn(uZone.GameInstance instance) {
		// Remove this from the waiting list so we don't get selected for a server again
		Match.matchesWaitingForServer.Remove(this);
		
		// Make all players connect to the game server
		foreach(List<LobbyPlayer> team in teams) {
			foreach(LobbyPlayer player in team) {
				player.ConnectToGameServer(instance);
			}
		}
		
		// Add this to the list of running matches
		Match.matchesRunning.Add(this);
	}
}
