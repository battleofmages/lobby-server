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
	
#if !LOBBY_SERVER
	// Loads a new map
	public static GameObject LoadMap(string mapName) {
		DeleteOldMap();
		
		LogManager.General.Log("Loading map: " + mapName);
		
		var mapPrefab = Resources.Load("Maps/" + mapName);
		var mapInstance = (GameObject)GameObject.Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
		
		// Update spawn locations
		Party.UpdateSpawns();
		
		// Update sun shafts caster
		if(!uLink.Network.isServer) {
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
		
		return mapInstance;
	}
	
	// Deletes existing map
	private static void DeleteOldMap() {
		var mapInstance = GameObject.FindGameObjectWithTag("Map");
		
		if(mapInstance == null)
			return;
		
		LogManager.General.Log("Deleting old map");
		GameObject.Destroy(mapInstance);
	}
#endif
}
