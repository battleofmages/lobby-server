public class LobbyWorld : LobbyGameInstance<LobbyWorld> {
	// Empty constructor
	private LobbyWorld() {
		// Set map pool
		if(LobbyTown.mapPool == null)
			LobbyTown.mapPool = MapManager.worlds;
		
		// Server type
		serverType = ServerType.World;
	}
	
	// Constructor
	public LobbyWorld(string nMapName) : this() {
		// Map name
		mapName = nMapName;
	}
}