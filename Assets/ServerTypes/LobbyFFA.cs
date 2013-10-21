using uLobby;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LobbyFFA : LobbyGameInstance<LobbyFFA> {
	public static int maxPlayerCount = 2;
	
	// Constructor
	public LobbyFFA(string nMapName) {
		// Set map pool
		if(LobbyTown.mapPool == null)
			LobbyTown.mapPool = MapManager.ffaMaps;
		
		// Server type
		serverType = ServerType.FFA;
		
		// Map name
		mapName = nMapName;
	}
	
	// Pick FFA instance
	public static LobbyFFA PickFFAInstance(LobbyPlayer player) {
		int lowestPlayerCount = 999999;
		LobbyFFA pickedInstance = null;
		
		// Look for the instance with the lowest amount of players
		foreach(var gameInstance in LobbyFFA.running) {
			var playerCount = gameInstance.players.Count;
			
			if(playerCount < lowestPlayerCount && playerCount < maxPlayerCount) {
				pickedInstance = (LobbyFFA)gameInstance;
				lowestPlayerCount = playerCount;
			}
		}
		
		// If no instance was found, we create one
		if(pickedInstance == null) {
			pickedInstance = new LobbyFFA(MapManager.ffaMaps[Random.Range(0, MapManager.ffaMaps.Length)]);
			pickedInstance.Register();
		}
		
		// Add player to instance already to prevent 2 persons connecting to the instance
		// when the server is directly below the maxPlayerCount.
		player.gameInstance = pickedInstance;
		
		return pickedInstance;
	}
	
	// ToString
	public override string ToString () {
		return string.Format("[LobbyFFA] {0}", mapName);
	}
}
