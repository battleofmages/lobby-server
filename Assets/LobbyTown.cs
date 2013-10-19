using uLobby;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LobbyTown : LobbyGameInstance<LobbyTown> {
	// Constructor
	public LobbyTown(string nMapName) {
		// Set map pool
		if(LobbyTown.mapPool == null)
			LobbyTown.mapPool = MapManager.towns;
		
		// Server type
		serverType = ServerType.Town;
		
		// Map name
		mapName = nMapName;
	}
	
	// ToString
	public override string ToString () {
		return string.Format("[LobbyTown] {0}", mapName);
	}
}
