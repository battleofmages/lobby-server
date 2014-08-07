public class LobbyWorld : LobbyGameInstance<LobbyWorld> {
	// Empty constructor
	private LobbyWorld() {
		// Server type
		_serverType = ServerType.World;
	}
	
	// Constructor
	public LobbyWorld(string nMapName) : this() {
		// Map name
		_mapName = nMapName;
	}
}