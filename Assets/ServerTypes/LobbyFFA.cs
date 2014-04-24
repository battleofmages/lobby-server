using UnityEngine;

public class LobbyFFA : LobbyGameInstance<LobbyFFA> {
	public const int maxPlayerCount = 10;
	
	// Constructor
	public LobbyFFA(string nMapName) {
		// Set map pool
		if(LobbyFFA.mapPool == null)
			LobbyFFA.mapPool = MapManager.ffaMaps;
		
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
		LogManager.General.Log("Trying to find an FFA instance for player '" + player.name + "'");
		foreach(var gameInstance in LobbyFFA.running) {
			var playerCount = gameInstance.players.Count;
			
			if(playerCount < lowestPlayerCount && playerCount < maxPlayerCount) {
				pickedInstance = (LobbyFFA)gameInstance;
				lowestPlayerCount = playerCount;
			}
		}
		
		// If no instance was found, we create one
		if(pickedInstance == null) {
			LogManager.General.Log("No free FFA instance found, creating one for '" + player.name + "'");
			pickedInstance = new LobbyFFA(MapManager.ffaMaps[Random.Range(0, MapManager.ffaMaps.Length)]);
			pickedInstance.Register();
		}
		
		// Add player to instance already to prevent 2 persons connecting to the instance
		// when the server is directly below the maxPlayerCount.
		player.gameInstance = pickedInstance;
		
		return pickedInstance;
	}
}
