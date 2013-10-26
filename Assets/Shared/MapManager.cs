using UnityEngine;
using System.Collections;

public class MapManager {
	// Starting town
	public static string defaultTown = "Nubek";
	
	// Towns
	public static string[] towns = new string[] {
		"Nubek",
		//"El Thea",
		//"Tamburin",
	};
	
	// Arenas
	public static string[] arenas = new string[] {
		"Ruins",
	};
	
	// FFA maps
	public static string[] ffaMaps = new string[] {
		"Nubek",
	};
	
#if !LOBBY_SERVER
	public static GameObject currentMapInstance;
	public static Intro currentMapIntro;
	
	// Loads a new map
	public static GameObject LoadMap(string mapName) {
		DeleteOldMap();
		
		LogManager.General.Log("Loading map: " + mapName);
		
		var mapPrefab = Resources.Load("Maps/" + mapName);
		LogManager.General.Log("Map prefab loaded");
		
		currentMapInstance = (GameObject)GameObject.Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
		LogManager.General.Log("Map instantiated: " + currentMapInstance);
		
		currentMapIntro = currentMapInstance.GetComponent<Intro>();
		LogManager.General.Log("Map intro: " + currentMapIntro);
		
		// Update spawn locations
		Party.UpdateSpawns();
		
		// Delete NPCs if needed
		if(GameManager.isFFA || GameManager.isArena) {
			DeleteNPCs();
		}
		
		// Update sun shafts caster
		/*if(isServer) {
			var sun = GameObject.FindGameObjectWithTag("Sun");
			var sunShafts = Camera.main.GetComponent<SunShafts>();
			if(sun != null && sunShafts != null) {
				// TODO: Why doesn't this work?
				sunShafts.sunTransform = sun.transform;
				LogManager.General.Log("Updated sun shafts caster to " + sun.ToString() + ", " + sun.transform.ToString());
			} else {
				LogManager.General.LogWarning("Couldn't find sun (did you use the 'Sun' tag?)");
			}
		}*/
		
		return currentMapInstance;
	}
	
	// Deletes NPCs
	public static void DeleteNPCs() {
		LogManager.General.Log("Deleting NPCs...");
		var npcList = GameObject.FindGameObjectsWithTag("NPC");
		
		foreach(var npc in npcList) {
			GameObject.Destroy(npc);
		}
		
		LogManager.General.Log("Finished deleting NPCs");
	}
	
	// Deletes existing map
	private static void DeleteOldMap() {
		currentMapInstance = GameObject.FindGameObjectWithTag("Map");
		
		if(currentMapInstance == null)
			return;
		
		LogManager.General.Log("Deleting old map");
		GameObject.Destroy(currentMapInstance);
		currentMapInstance = null;
	}
	
	public static void InitPhysics(ServerType serverType) {
		LogManager.General.Log("Initializing map physics");
		
		if(serverType == ServerType.Town) {
			//Physics.IgnoreLayerCollision(Party.partyList[0].layer, Party.partyList[0].layer, false);
		} else {
			//Physics.IgnoreLayerCollision(Party.partyList[0].layer, Party.partyList[0].layer, true);
		}
	}
#endif
}
