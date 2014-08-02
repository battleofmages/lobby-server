public class LobbyTown : LobbyGameInstance<LobbyTown> {
	// Empty constructor
	private LobbyTown() {
		// Server type
		serverType = ServerType.Town;
	}
	
	// Constructor
	public LobbyTown(string nMapName) : this() {
		// Map name
		mapName = nMapName;
	}
}
