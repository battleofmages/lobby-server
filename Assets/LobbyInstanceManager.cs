using uZone;
using UnityEngine;
using System.Collections.Generic;

public class LobbyInstanceManager : SingletonMonoBehaviour<LobbyInstanceManager> {
	public static bool uZoneConnected;
	public static int uZoneNodeCount;
	public static string gameName = "bom";
	
	public string uZoneHost = "127.0.0.1";
	public int uZonePort = 12345;
	public NodeSelectionMode nodeSelectionMode;
	
	// Start
	void Start () {
		// Initialize uZone
		InstanceManager.GlobalEvents events = new InstanceManager.GlobalEvents();
		events.onDisconnected = uZone_OnDisconnected;
		events.onNodeConnected = uZone_OnNodeConnected;
		events.onNodeDisconnected = uZone_OnNodeDisconnected;
		events.onInstanceStarted = uZone_OnInstanceStarted;
		events.onInstanceStopped = uZone_OnInstanceStopped;
		
		InstanceManager.Initialize(events);
		Reconnect();
	}
	
	// Reconnect
	void Reconnect() {
		InstanceManager.Connect(uZoneHost, uZonePort, uZone_OnConnected, uZone_OnConnectFailed);
	}
	
	// GetGameInstanceByID
	public static LobbyGameInstanceInterface GetGameInstanceByID(string instanceIdNumber) {
		return
			GetGameInstanceByID<LobbyMatch>(instanceIdNumber) ??
			GetGameInstanceByID<LobbyWorld>(instanceIdNumber) ??
			GetGameInstanceByID<LobbyTown>(instanceIdNumber) ??
			GetGameInstanceByID<LobbyFFA>(instanceIdNumber) ??
			null;
	}
	
	// GetGameInstanceByID<T>
	public static LobbyGameInstanceInterface GetGameInstanceByID<T>(string instanceIdNumber) {
		foreach(KeyValuePair<InstanceID, LobbyGameInstance<T>> entry in LobbyGameInstance<T>.idToInstance) {
			if(entry.Key.ToString().Split(' ')[1] == instanceIdNumber)
				return entry.Value;
		}
		
		return default(LobbyGameInstance<T>);
	}
	
	// Unregister game instance
	public bool UnregisterGameInstance<T>(InstanceID id, Dictionary<InstanceID, LobbyGameInstance<T>> idToInstance) where T : LobbyGameInstance<T> {
		LobbyGameInstance<T> inst;
		
		if(idToInstance.TryGetValue(id, out inst)) {
			inst.Unregister();
			return true;
		}
		
		return false;
	}
	
	// uZone connection established
	void uZone_OnConnected(Request request) {
		LogManager.General.Log("Connected to uZone: " + request);
		
		LobbyInstanceManager.uZoneConnected = true;
		uZoneNodeCount = new List<Node>(InstanceManager.nodes).Count;
		LogManager.General.Log(uZoneNodeCount + " node(s) connected");
	}
	
	// uZone connection error
	void uZone_OnConnectFailed(Request request) {
		LogManager.General.LogError("Failed to connect to uZone: " + request);
		
		// Try again
		Reconnect();
	}
	
	// uZone disconnect
	void uZone_OnDisconnected() {
		LogManager.General.LogError("Disconnected from uZone");
		
		// Try again
		Reconnect();
	}
	
	// uZone node connection established
	void uZone_OnNodeConnected(Node node) {
		LogManager.General.Log("Connected to uZone node (" + node + ")");
		
		// Start matchmaking after downtime of uZone nodes
		var queue = LobbyMatchMaker.instance.queue;
		if(queue != null && LobbyInstanceManager.uZoneNodeCount == 0) {
			foreach(var q in queue) {
				q.MakeMatchesBasedOnRanking();
			}
		}
		
		LobbyInstanceManager.uZoneNodeCount += 1;
	}
	
	// uZone node connection lost
	void uZone_OnNodeDisconnected(Node node) {
		LogManager.General.LogError("Lost connection to uZone node: " + node);
		LobbyInstanceManager.uZoneNodeCount -= 1;
	}
	
	// uZone node list
	void uZone_OnNodeListReceived(List<Node> newNodeList) {
		LogManager.General.Log("Received uZone node list (" + newNodeList.Count + " online).");
		
		foreach(var node in newNodeList) {
			LogManager.General.Log(node.ToString());
		}
		
		LobbyInstanceManager.uZoneNodeCount = newNodeList.Count;
		
		// Start town servers
		//StartTownServers();
	}
	
	// A new game server has finished starting up
	void uZone_OnInstanceStarted(InstanceProcess instance) {
		// ...
	}
	
	// A game server stopped running
	void uZone_OnInstanceStopped(InstanceProcess instance) {
		LogManager.General.Log("Instance stopped running: " + instance);
		
		if(UnregisterGameInstance<LobbyMatch>(instance.id, LobbyMatch.idToInstance))
			return;
		
		if(UnregisterGameInstance<LobbyWorld>(instance.id, LobbyWorld.idToInstance))
			return;
		
		if(UnregisterGameInstance<LobbyTown>(instance.id, LobbyTown.idToInstance))
			return;
		
		if(UnregisterGameInstance<LobbyFFA>(instance.id, LobbyFFA.idToInstance))
			return;
	}
}
