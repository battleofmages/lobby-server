using System.Linq;
using System.Collections.Generic;

public interface LobbyGameInstanceInterface {
	List<LobbyPlayer> players{ get; }
	LobbyChatChannel mapChannel { get; }
	
	void Register();
	void Unregister();
}

public abstract class LobbyGameInstance<T> : LobbyGameInstanceInterface {
	public static List<LobbyGameInstance<T>> waitingForServer = new List<LobbyGameInstance<T>>();
	public static List<LobbyGameInstance<T>> running = new List<LobbyGameInstance<T>>();
	public static Dictionary<uZone.InstanceID, LobbyGameInstance<T>> idToInstance = new Dictionary<uZone.InstanceID, LobbyGameInstance<T>>();
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
	public List<string> args = new List<string>();
	public int maxPlayerCount = 999999999;
	protected string mapName;
	protected ServerType serverType;
	private uZone.InstanceOptions options;
	
	// Requests uZone to start a new instance
	protected virtual void StartInstanceAsync() {
		args.Add("-type" + serverType);
		args.Add("\"-map" + mapName + "\"");
		
		// Number of parties
		int partyCount = 1;
		
		switch(serverType) {
			case ServerType.Arena:
				partyCount = 2;
				break;
				
			case ServerType.FFA:
				partyCount = 10;
				break;
		}
		
		args.Add("-partycount" + partyCount);
		
		// Add to list by map name
		if(!mapNameToInstances.ContainsKey(mapName)) {
			mapNameToInstances[mapName] = new List<LobbyGameInstance<T>>();
		}
		
		mapNameToInstances[mapName].Add(this);
		
		waitingForServer.Add(this);
		options = new uZone.InstanceOptions(LobbyInstanceManager.gameName, args);
		
		// Request uZone to start a new instance
		uZone.InstanceManager.StartInstance(
			options,
			LobbyInstanceManager.instance.nodeSelectionMode,
			(request) => {
				// Success
				instance = request.GetInstance();
				idToInstance[instance.id] = this;
				StartPlaying();
			}, (request) => {
				// Failure
				LogManager.General.LogError("Couldn't start instance: " + request);
			}
		);
	}
	
	// Starts playing on game server instance
	void StartPlaying() {
		// Log after the instance has been assigned, so we see the IP
		LogManager.General.Log("Instance started: " + this);
		
		// Remove this from the waiting list so we don't get selected for a server again
		waitingForServer.Remove(this);
		
		// Create a map channel
		var channelName = "Map@" + instance.node.publicAddress + ":" + instance.port;
		
		LogManager.Chat.Log("Creating chat channel: " + channelName);
		_mapChannel = new LobbyChatChannel(channelName);
		
		// Callback
		OnInstanceAvailable();
		
		// Add this to the list of running instances
		running.Add(this);
	}
	
	// Register
	public virtual void Register() {
		// Custom callback
		OnRegister();
		
		// Async: Start game server instance for this match
		StartInstanceAsync();
	}
	
	// Unregister
	public virtual void Unregister() {
		LogManager.General.Log("Unregistering instance: " + this);
		
		//this.OnUnregister();
		
		_mapChannel.Unregister();
		_mapChannel = null;
		
		if(!idToInstance.Remove(instance.id))
			LogManager.General.LogError("Could not unregister instance id " + instance.id);
		
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
					player.ReturnToWorld();
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
			from p in players
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
