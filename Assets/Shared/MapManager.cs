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
	public static GameObject mapInstance;
	public static Intro mapIntro;
	public static Bounds mapBounds;
	
	// Loads a new map
	public static GameObject LoadMap(string mapName) {
		DeleteOldMap();
		
		LogManager.General.Log("Loading map: " + mapName);
		
		var mapPrefab = Resources.Load("Maps/" + mapName);
		LogManager.General.Log("Map prefab loaded");
		
		mapInstance = (GameObject)GameObject.Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
		LogManager.General.Log("Map instantiated: " + mapInstance);
		
		mapIntro = mapInstance.GetComponent<Intro>();
		LogManager.General.Log("Map intro: " + mapIntro);
		
		mapBounds = mapInstance.GetComponent<Boundary>().bounds;
		LogManager.General.Log("Map bounds: " + mapBounds);
		
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
		
		return mapInstance;
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
		mapInstance = GameObject.FindGameObjectWithTag("Map");
		
		if(mapInstance == null)
			return;
		
		LogManager.General.Log("Deleting old map");
		GameObject.Destroy(mapInstance);
		mapInstance = null;
	}
	
	// Stay in map boundaries
	public static Vector3 StayInMapBoundaries(Vector3 pos) {
		Vector3 min = MapManager.mapBounds.min;
		Vector3 max = MapManager.mapBounds.max;
		
		if(pos.x < min.x)
			pos.Set(min.x, pos.y, pos.z);
		else if(pos.x > max.x)
			pos.Set(max.x, pos.y, pos.z);
		
		if(pos.y < min.y)
			pos.Set(pos.x, min.y, pos.z);
		else if(pos.y > max.y)
			pos.Set(pos.x, max.y, pos.z);
		
		if(pos.z < min.z)
			pos.Set(pos.x, pos.y, min.z);
		else if(pos.z > max.z)
			pos.Set(pos.x, pos.y, max.z);
		
		return pos;
	}
	
	// Init physics
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
