using UnityEngine;
using System.Collections;

public class MapManager {
	public static GameObject currentMapInstance;
	
#if !LOBBY_SERVER
	public static Intro currentMapIntro;
#endif
	
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
	// Loads a new map
	public static GameObject LoadMap(string mapName) {
		DeleteOldMap();
		
		LogManager.General.Log("Loading map: " + mapName);
		
		var mapPrefab = Resources.Load("Maps/" + mapName);
		currentMapInstance = (GameObject)GameObject.Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
		currentMapIntro = currentMapInstance.GetComponent<Intro>();
		
		// Update spawn locations
		Party.UpdateSpawns();
		
		// Update sun shafts caster
		if(uLink.Network.isClient) {
			var sun = GameObject.FindGameObjectWithTag("Sun");
			var sunShafts = Camera.main.GetComponent<SunShafts>();
			if(sun != null && sunShafts != null) {
				// TODO: Why doesn't this work?
				sunShafts.sunTransform = sun.transform;
				LogManager.General.Log("Updated sun shafts caster to " + sun.ToString() + ", " + sun.transform.ToString());
			} else {
				LogManager.General.LogWarning("Couldn't find sun (did you use the 'Sun' tag?)");
			}
		}
		
		return currentMapInstance;
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
		if(serverType == ServerType.Town) {
			//Physics.IgnoreLayerCollision(Party.partyList[0].layer, Party.partyList[0].layer, false);
		} else {
			//Physics.IgnoreLayerCollision(Party.partyList[0].layer, Party.partyList[0].layer, true);
		}
	}
#endif
}
