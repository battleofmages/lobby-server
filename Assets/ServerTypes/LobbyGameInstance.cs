using uLobby;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public interface LobbyGameInstanceInterface {
	List<LobbyPlayer> players{ get; }
	LobbyChatChannel mapChannel { get; }
	
	//void StartInstanceAsync();
	void StartPlayingOn(uZone.InstanceProcess newInstance);
	void Register();
	void Unregister();
	//void OnRegister();
	//void OnUnregister();
	//void OnInstanceAvailable();
}

public abstract class LobbyGameInstance<T> : LobbyGameInstanceInterface {
	public static List<LobbyGameInstance<T>> waitingForServer = new List<LobbyGameInstance<T>>();
	public static List<LobbyGameInstance<T>> running = new List<LobbyGameInstance<T>>();
	public static Dictionary<uZone.InstanceID, LobbyGameInstance<T>> idToInstance = new Dictionary<uZone.InstanceID, LobbyGameInstance<T>>();
	public static Dictionary<uZone.InstanceID, LobbyGameInstance<T>> requestIdToInstance = new Dictionary<uZone.InstanceID, LobbyGameInstance<T>>();
	public static Dictionary<string, List<LobbyGameInstance<T>>> mapNameToInstances = new Dictionary<string, List<LobbyGameInstance<T>>>();
	public static string[] mapPool = null;
	
	private List<LobbyPlayer> _players = new List<LobbyPlayer>();
	private LobbyChatChannel _mapChannel;
	
	public List<LobbyPlayer> players {
		get { return _players; }
	}
	
	public LobbyChatChannel mapChannel {
		get { return _mapChannel; }
	}
	
	public uZone.InstanceProcess instance = null;
	public uZone.InstanceID requestId;
	public List<string> args = new List<string>();
	protected string mapName;
	protected ServerType serverType;
	private uZone.InstanceOptions options;
	
	// Requests uZone to start a new instance
	protected virtual void StartInstanceAsync() {
		args.Add("-type" + serverType.ToString());
		args.Add("\"-map" + mapName + "\"");
		
		// Add to list by map name
		if(!mapNameToInstances.ContainsKey(mapName)) {
			mapNameToInstances[mapName] = new List<LobbyGameInstance<T>>();
		}
		
		mapNameToInstances[mapName].Add(this);
		
		waitingForServer.Add(this);
		options = new uZone.InstanceOptions(LobbyServer.gameName, args);
		
		var node = uZone.InstanceManager.nodes.FirstOrDefault();
		node.StartInstance(options, (request) => {
			instance = request.GetInstance();
			requestId = instance.id;
			requestIdToInstance[requestId] = this;
		}, (request) => {
			
		});
		
		/*yield return startRequest.WaitUntilDone();
		startRequest.TryGetInstance(out instance);*/
		
		//requestId = uZone.InstanceManager.StartGameInstance(LobbyServer.gameName, args);
		//requestIdToInstance[requestId] = this;
	}
	
	// Starts playing on game server instance
	public virtual void StartPlayingOn(uZone.InstanceProcess newInstance) {
		instance = newInstance;
		
		// Log after the instance has been assigned, so we see the IP
		LogManager.General.Log("Instance started: " + this.ToString());
		
		// Remove this from the waiting list so we don't get selected for a server again
		waitingForServer.Remove(this);
		
		// Create a map channel
		_mapChannel = new LobbyChatChannel("Map@" + instance.node.publicAddress + ":" + instance.port);
		
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
		LogManager.General.Log("Instance stopped running: " + this.ToString());
		
		//this.OnUnregister();
		
		_mapChannel.Unregister();
		_mapChannel = null;
		
		if(!requestIdToInstance.Remove(requestId))
			LogManager.General.LogError("Could not unregister request id " + requestId);
		
		if(!idToInstance.Remove(instance.id))
			LogManager.General.LogError("Could not unregister instance id " + requestId);
		
		if(!mapNameToInstances[mapName].Remove(this))
			LogManager.General.LogError("Could not unregister instance from map name list: " + mapName + ", " + this.ToString());
		
		if(!running.Remove(this))
			LogManager.General.LogError("Could not unregister instance from the running list: " + this.ToString());
		
		// Redirect players
		var playerList = new List<LobbyPlayer>(players);
		int reconnectCount = 0;
		foreach(var player in playerList) {
			if(player.gameInstance == this) {
				if(player.inTown) {
					player.gameInstance = null;
					LobbyServer.instance.ReturnPlayerToTown(player);
					reconnectCount++;
				} else {
					// ...
					player.gameInstance = null;
				}
			}
		}
		
		// In that case we'll send the players back to the town server.
		// This can happen if you connect to a server while it is shutting down.
		if(reconnectCount > 0)
			LogManager.General.Log(string.Format("Server crashed, returned {0} players on the instance to the town.", reconnectCount));
	}
	
	// ToString
	public override string ToString() {
		var playerList =
			from p in this.players
			select p.ToString();
		var playerListString = string.Join(", ", playerList.ToArray());
		
		if(instance != null) {
			return string.Format("[{0}] {1}\n * {2}:{3}\n * Players: [{4}]", serverType.ToString(), mapName, instance.node.publicAddress, instance.port, playerListString);
		}
		
		return string.Format("[{0}] {1}\n * Players: [{2}]", serverType.ToString(), mapName, playerListString);
	}
	
	protected virtual void OnRegister() {}
	//protected virtual void OnUnregister() {}
	protected virtual void OnInstanceAvailable() {}
}
