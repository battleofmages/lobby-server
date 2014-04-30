public class LobbyWorld : LobbyGameInstance<LobbyWorld> {
	// Constructor
	public LobbyWorld(string nMapName) {
		// Server type
		serverType = ServerType.World;
		
		// Map name
		mapName = nMapName;
	}
}