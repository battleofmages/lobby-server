using uLobby;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class LobbyGameInstance<T> {
	public static List<LobbyGameInstance<T>> waitingForServer = new List<LobbyGameInstance<T>>();
	public static List<LobbyGameInstance<T>> running = new List<LobbyGameInstance<T>>();
	public static Dictionary<string, LobbyGameInstance<T>> idToInstance = new Dictionary<string, LobbyGameInstance<T>>();
	public static Dictionary<int, LobbyGameInstance<T>> requestIdToInstance = new Dictionary<int, LobbyGameInstance<T>>();
	public static Dictionary<string, List<LobbyGameInstance<T>>> mapNameToInstances = new Dictionary<string, List<LobbyGameInstance<T>>>();
	public static string[] mapPool = null;
	public List<LobbyPlayer> players = new List<LobbyPlayer>();
	
	public uZone.GameInstance instance = null;
	public int requestId;
	public LobbyChatChannel mapChannel;
	public List<string> args = new List<string>();
	protected string mapName;
	protected ServerType serverType;
	
	// Requests uZone to start a new instance
	protected virtual void StartInstanceAsync() {
		args.Add("-type" + serverType.ToString());
		args.Add("-map" + mapName);
		
		// Add to list by map name
		if(!mapNameToInstances.ContainsKey(mapName)) {
			mapNameToInstances[mapName] = new List<LobbyGameInstance<T>>();
		}
		
		mapNameToInstances[mapName].Add(this);
		
		waitingForServer.Add(this);
		requestId = uZone.InstanceManager.StartGameInstance(LobbyServer.gameName, args);
		requestIdToInstance[requestId] = this;
	}
	
	// Starts playing on game server instance
	public virtual void StartPlayingOn(uZone.GameInstance newInstance) {
		instance = newInstance;
		
		// Remove this from the waiting list so we don't get selected for a server again
		waitingForServer.Remove(this);
		
		// Create a map channel
		mapChannel = new LobbyChatChannel("Map@" + instance.ip + ":" + instance.port);
		
		// Callback
		this.OnInstanceAvailable();
		
		// Add this to the list of running instances
		idToInstance[instance.id] = this;
		running.Add(this);
	}
	
	// Register
	public virtual void Register() {
		// Custom callback
		this.OnRegister();
		
		// Async: Start game server instance for this match
		this.StartInstanceAsync();
	}
	
	// Unregister
	public virtual void Unregister() {
		LogManager.General.Log("Unregistering instance '" + instance.id + "'...");
		
		//this.OnUnregister();
		
		mapChannel.Unregister();
		mapChannel = null;
		
		if(!requestIdToInstance.Remove(requestId))
			LogManager.General.LogError("Could not unregister request id " + requestId);
		
		if(!idToInstance.Remove(instance.id))
			LogManager.General.LogError("Could not unregister instance id " + requestId);
		
		if(!mapNameToInstances[mapName].Remove(this))
			LogManager.General.LogError("Could not unregister instance from map name list: " + mapName + ", " + this.ToString());
		
		if(!running.Remove(this))
			LogManager.General.LogError("Could not unregister instance from the running list: " + this.ToString());
		
		// Redirect players
		foreach(var player in players) {
			if(player.gameInstance == this) {
				// Town server crashed?
				if(player.inTown) {
					// In that case we'll restart the town server.
					// This can happen if you connect to a server while it is shutting down.
					LogManager.General.Log("Town server crashed, restarting it and reconnecting all players to the new server.");
					
					player.town = null;
					LobbyServer.instance.ReturnPlayerToTown(player);
				}
			}
		}
	}
	
	protected virtual void OnRegister() {}
	//protected virtual void OnUnregister() {}
	protected virtual void OnInstanceAvailable() {}
}
