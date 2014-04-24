public class LobbyPvE : LobbyGameInstance<LobbyPvE> {
	// Constructor
	public LobbyPvE(string nMapName) {
		// Server type
		serverType = ServerType.PvE;
		
		// Map name
		mapName = nMapName;
	}
}