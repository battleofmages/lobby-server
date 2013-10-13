using uLobby;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LobbyMatch : LobbyGameInstance<LobbyMatch> {
	public List<LobbyPlayer>[] teams;
	public bool updatedRankingList = false;
	
	// Constructor
	public LobbyMatch() {
		// Set map pool
		if(LobbyMatch.mapPool == null)
			LobbyMatch.mapPool = MapManager.arenas;
		
		// Server type
		serverType = ServerType.Arena;
		
		// Map name
		mapName = LobbyMatch.mapPool[Random.Range(0, mapPool.Length)];
		
		// Create the player lists
		teams = new List<LobbyPlayer>[4];
		
		for(int i = 0; i < teams.Length; i++) {
			teams[i] = new List<LobbyPlayer>();
		}
	}
	
	// Before registering
	protected override void OnRegister() {
		// Prepare arguments: -team0 account1 account2 -team1 account3 account4
		for(int i = 0; i < teams.Length; i++) {
			args.Add("-party" + i);
			
			foreach(LobbyPlayer player in teams[i]) {
				args.Add(player.accountId);
			}
		}
	}
	
	// This is what we do when an instance becomes available
	protected override void OnInstanceAvailable() {
		// Make all players connect to the game server
		foreach(List<LobbyPlayer> team in teams) {
			foreach(LobbyPlayer player in team) {
				player.ConnectToGameInstance(this);
			}
		}
	}
}