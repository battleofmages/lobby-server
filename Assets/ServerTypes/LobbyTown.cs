public class LobbyTown : LobbyGameInstance<LobbyTown> {
	// Empty constructor
	private LobbyTown() {
		// Set map pool
		if(LobbyTown.mapPool == null)
			LobbyTown.mapPool = MapManager.towns;
		
		// Server type
		serverType = ServerType.Town;
	}
	
	// Constructor
	public LobbyTown(string nMapName) : this() {
		// Map name
		mapName = nMapName;
	}
}
