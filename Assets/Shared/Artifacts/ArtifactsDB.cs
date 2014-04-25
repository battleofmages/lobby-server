using uLobby;
using System.Collections;

public static class ArtifactsDB {
	// --------------------------------------------------------------------------------
	// AccountToArtifactTree
	// --------------------------------------------------------------------------------
	
	// Get artifact tree
	public static IEnumerator GetArtifactTree(string accountId, GameDB.ActionOnResult<ArtifactTree> func) {
		yield return GameDB.instance.StartCoroutine(GameDB.Get<ArtifactTree>(
			"AccountToArtifactTree",
			accountId,
			func
		));
	}
	
	// --------------------------------------------------------------------------------
	// AccountToArtifactInventory
	// --------------------------------------------------------------------------------
	
	// Get artifact inventory
	public static IEnumerator GetArtifactInventory(string accountId, GameDB.ActionOnResult<ArtifactInventory> func) {
		yield return GameDB.instance.StartCoroutine(GameDB.Get<ArtifactInventory>(
			"AccountToArtifactInventory",
			accountId,
			func
		));
	}
	
#if LOBBY_SERVER
	// Get artifact tree
	public static IEnumerator GetArtifactTree(LobbyPlayer player) {
		yield return GameDB.instance.StartCoroutine(GameDB.Get<ArtifactTree>(
			"AccountToArtifactTree",
			player.accountId,
			data => {
				if(data == null) {
					player.artifactTree = ArtifactTree.GetStarterArtifactTree();
				} else {
					player.artifactTree = data;
				}
				
				Lobby.RPC("ReceiveArtifactTree", player.peer, player.accountId, Jboy.Json.WriteObject(player.artifactTree));
			}
		));
	}
	
	// Set artifact tree
	public static IEnumerator SetArtifactTree(LobbyPlayer player, ArtifactTree tree) {
		yield return GameDB.instance.StartCoroutine(GameDB.Set<ArtifactTree>(
			"AccountToArtifactTree",
			player.accountId,
			tree,
			data => {
				if(data == null)
					Lobby.RPC("ArtifactTreeSaveError", player.peer);
				else
					player.artifactTree = data;
			}
		));
	}
	
	// --------------------------------------------------------------------------------
	// AccountToArtifactInventory
	// --------------------------------------------------------------------------------
	
	// Get artifact inventory
	public static IEnumerator GetArtifactInventory(LobbyPlayer player) {
		yield return GameDB.instance.StartCoroutine(GameDB.Get<ArtifactInventory>(
			"AccountToArtifactInventory",
			player.accountId,
			data => {
				if(data == null) {
					player.artifactInventory = new ArtifactInventory();
				} else {
					player.artifactInventory = data;
				}
				
				Lobby.RPC("ReceiveArtifactInventory", player.peer, player.accountId, Jboy.Json.WriteObject(player.artifactInventory));
			}
		));
	}
	
	// Set artifact tree
	public static IEnumerator SetArtifactInventory(LobbyPlayer player, ArtifactInventory inv) {
		yield return GameDB.instance.StartCoroutine(GameDB.Set<ArtifactInventory>(
			"AccountToArtifactInventory",
			player.accountId,
			inv,
			data => {
				if(data == null)
					Lobby.RPC("ArtifactInventorySaveError", player.peer);
				else
					player.artifactInventory = data;
			}
		));
	}
#endif
}
