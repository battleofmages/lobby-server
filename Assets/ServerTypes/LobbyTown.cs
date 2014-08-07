public class LobbyTown : LobbyGameInstance<LobbyTown> {
	// Empty constructor
	private LobbyTown() {
		// Server type
		_serverType = ServerType.Town;
	}
	
	// Constructor
	public LobbyTown(string nMapName) : this() {
		// Map name
		_mapName = nMapName;
	}
}
