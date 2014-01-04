using UnityEngine;
using System.IO;

public class MapManager {
	// Starting town
	public static string defaultTown = "Alpha Tester Grounds";
	
	// Towns
	public static string[] towns = new string[] {
		"Alpha Tester Grounds",
		//"Nubek"
		//"El Thea",
		//"Tamburin",
	};
	
	// Arenas
	public static string[] arenas = new string[] {
		//"Alpha Tester Grounds",
		"Ruins"
	};
	
	// FFA maps
	public static string[] ffaMaps = new string[] {
		"Alpha Tester Grounds",
		//"Nubek",
	};
	
#if !LOBBY_SERVER
	public static GameObject mapInstance;
	public static Intro mapIntro;
	public static Bounds mapBounds;
	public static bool occlusionCullingActive = false;
	public static Transform occlusionArea = null;
	
	// Loads a new map
	public static GameObject LoadMap(string mapName) {
		DeleteOldMap();
		
		LogManager.General.Log("Loading map: " + mapName);
		
		var mapPrefab = Resources.Load("Maps/" + mapName);
		LogManager.General.Log("Map prefab loaded");
		
		mapInstance = (GameObject)GameObject.Instantiate(mapPrefab, Cache.vector3Zero, Cache.quaternionIdentity);
		LogManager.General.Log("Map instantiated: " + mapInstance);
		
		mapIntro = mapInstance.GetComponent<Intro>();
		LogManager.General.Log("Map intro: " + mapIntro);
		
		mapBounds = mapInstance.GetComponent<Boundary>().bounds;
		LogManager.General.Log("Map bounds: " + mapBounds);
		
		occlusionArea = mapInstance.transform.FindChild("Occlusion Area");
		if(occlusionArea != null) {
			LogManager.General.Log("Occlusion culling information available");
			occlusionCullingActive = true;
		} else {
			LogManager.General.Log("Occlusion culling information not available");
			occlusionCullingActive = false;
		}
		
		// Update spawn locations
		GameServerParty.UpdateSpawns();
		
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
			//Physics.IgnoreLayerCollision(Party.partyList[0].layer, GameServerParty.partyList[0].layer, false);
		} else {
			//Physics.IgnoreLayerCollision(Party.partyList[0].layer, GameServerParty.partyList[0].layer, true);
		}
	}
#endif
}
