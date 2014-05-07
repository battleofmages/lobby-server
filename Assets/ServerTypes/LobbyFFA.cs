using UnityEngine;
using System.Collections.Generic;

public class LobbyFFA : LobbyGameInstance<LobbyFFA> {
	// Constructor
	public LobbyFFA(string nMapName) {
		// Set map pool
		if(LobbyFFA.mapPool == null)
			LobbyFFA.mapPool = MapManager.ffaMaps;
		
		// Server type
		serverType = ServerType.FFA;
		
		// Map name
		mapName = nMapName;
		
		// Set max player count
		maxPlayerCount = 10;
	}
	
	// PickInstanceWithLeastPlayers
	public static T PickInstanceWithLeastPlayers<T>(List<LobbyGameInstance<T>> instanceList) where T : LobbyGameInstance<T> {
		int lowestPlayerCount = 999999;
		T pickedInstance = default(T);
		
		foreach(var gameInstance in instanceList) {
			var playerCount = gameInstance.players.Count;
			
			if(playerCount < lowestPlayerCount && playerCount < gameInstance.maxPlayerCount) {
				pickedInstance = (T)gameInstance;
				lowestPlayerCount = playerCount;
			}
		}
		
		return pickedInstance;
	}
	
	// Pick FFA instance
	public static LobbyFFA PickFFAInstance(LobbyPlayer player) {
		LobbyFFA pickedInstance = null;
		
		// Look for the instance with the lowest amount of players
		LogManager.General.Log("[PickFFAInstance] Trying to find a running FFA instance for player '" + player.name + "'");
		pickedInstance = PickInstanceWithLeastPlayers(LobbyFFA.running);
		
		// If no instance was found, try the waitingForServer list if someone else registered already
		if(pickedInstance == null) {
			LogManager.General.Log("[PickFFAInstance] No free FFA instance found for '" + player.name + "', checking if someone started one already");
			pickedInstance = PickInstanceWithLeastPlayers(LobbyFFA.waitingForServer);
		}
		
		// If no instance was found, we create one
		if(pickedInstance == null) {
			LogManager.General.Log("[PickFFAInstance] No free FFA instance found, creating one for '" + player.name + "'");
			pickedInstance = new LobbyFFA(MapManager.ffaMaps[Random.Range(0, MapManager.ffaMaps.Length)]);
			pickedInstance.Register();
		}
		
		// Add player to instance already to prevent 2 persons connecting to the instance
		// when the server is directly below the maxPlayerCount.
		LogManager.General.Log("[PickFFAInstance] '" + player.name + "' will be connected to " + pickedInstance);
		player.gameInstance = pickedInstance;
		
		return pickedInstance;
	}
}
