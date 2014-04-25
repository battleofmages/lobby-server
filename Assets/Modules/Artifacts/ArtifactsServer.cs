using UnityEngine;
using System.Collections;
using uLobby;

public class ArtifactsServer : MonoBehaviour {
	// Start
	void Start () {
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void ClientArtifactTree(string jsonTree, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		//LogManager.General.Log(jsonTree);
		
		ArtifactTree tree = Jboy.Json.ReadObject<ArtifactTree>(jsonTree);
		
		LogManager.General.Log("Player '" + player.name + "' sent new artifact tree " + tree.ToString());
		StartCoroutine(ArtifactsDB.SetArtifactTree(player, tree));
	}
	
	[RPC]
	IEnumerator ClientArtifactEquip(int itemId, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		// Wait for all actions to execute
		while(player.artifactsEditingFlag) {
			yield return null;
		}
		
		try {
			var arti = new Artifact(itemId);
			
			player.artifactsEditingFlag = true;
			if(player.artifactTree.AddArtifact(itemId)) {
				player.artifactInventory.RemoveArtifact(arti);
				Lobby.RPC("ArtifactEquip", player.peer, itemId);
				
				// Save changes
				yield return StartCoroutine(ArtifactsDB.SetArtifactTree(
					player,
					player.artifactTree
				));
				
				yield return StartCoroutine(ArtifactsDB.SetArtifactInventory(
					player,
					player.artifactInventory
				));
			}
		} finally {
			player.artifactsEditingFlag = false;
		}
	}
	
	[RPC]
	IEnumerator ClientArtifactUnequip(byte level, byte slotIndex, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		// Wait for all actions to execute
		while(player.artifactsEditingFlag) {
			yield return null;
		}
		
		var slot = player.artifactTree.slots[level][slotIndex];
		if(slot.artifact == null)
			yield break;
		
		try {
			player.artifactsEditingFlag = true;
			player.artifactInventory.AddArtifact(slot.artifact);
			slot.artifact = null;
			Lobby.RPC("ArtifactUnequip", player.peer, level, slotIndex);
			
			// Save changes
			yield return StartCoroutine(ArtifactsDB.SetArtifactTree(
				player,
				player.artifactTree
			));
			
			yield return StartCoroutine(ArtifactsDB.SetArtifactInventory(
				player,
				player.artifactInventory
			));
		} finally {
			player.artifactsEditingFlag = false;
		}
	}
	
	[RPC]
	IEnumerator ClientArtifactDiscard(byte level, byte slotId, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		// Wait for all actions to execute
		while(player.artifactsEditingFlag) {
			yield return null;
		}
		
		try {
			player.artifactsEditingFlag = true;
			player.artifactInventory.bags[level].RemoveItemSlot(slotId);
			Lobby.RPC("ArtifactDiscard", player.peer, level, slotId);
			
			// Save changes
			yield return StartCoroutine(ArtifactsDB.SetArtifactTree(
				player,
				player.artifactTree
			));
			
			yield return StartCoroutine(ArtifactsDB.SetArtifactInventory(
				player,
				player.artifactInventory
			));
		} finally {
			player.artifactsEditingFlag = false;
		}
	}
	
	/*[RPC]
	IEnumerator ClientArtifactTreeSave(LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		// Save changes
		yield return StartCoroutine(artifactsDB.SetArtifactTree(
			player,
			player.artifactTree
		));
		
		yield return StartCoroutine(artifactsDB.SetArtifactInventory(
			player,
			player.artifactInventory
		));
		
		Lobby.RPC("ArtifactTreeSaveSuccess", player.peer);
	}*/
}
