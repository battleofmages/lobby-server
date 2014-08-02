public class LobbyWorld : LobbyGameInstance<LobbyWorld> {
	// Empty constructor
	private LobbyWorld() {
		// Server type
		serverType = ServerType.World;
	}
	
	// Constructor
	public LobbyWorld(string nMapName) : this() {
		// Map name
		mapName = nMapName;
	}
}