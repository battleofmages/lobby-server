using UnityEngine;
using uLobby;

public class ChatServer : MonoBehaviour {
	// Start
	void Start () {
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// User and admin commands
	bool ProcessLobbyChatCommands(LobbyPlayer player, string msg) {
		msg = msg.ReplaceCommands();
		
		switch(msg) {
			case "//practice":
				if(!player.inMatch) {
					LobbyQueue.CreatePracticeMatch(player);
					//match.Register();
				} else {
					// Notify player ...
				}
				return true;
				
			default:
				if(msg.StartsWith("//list ")) {
					//var serverType = msg.Substring(7);
				} else if(msg.StartsWith("//goto ")) {
					// Only for CMs
					if(player.accessLevel < AccessLevel.CommunityManager)
						return false;
					
					var param = msg.Substring("//goto ".Length);
					var spacePos = param.IndexOf(' ');
					var serverTypeString = param.Substring(0, spacePos);
					var mapName = param.Substring(spacePos + 1);
					
					player.location = new PlayerLocation(mapName, GetServerType(serverTypeString));
				} else if(msg.StartsWith("//movetoplayer ")) {
					// Only for CMs
					if(player.accessLevel < AccessLevel.CommunityManager)
						return false;
					
					var playerName = msg.Substring("//movetoplayer ".Length);
					
					LobbyGameDB.GetAccountIdByPlayerName(playerName, accountId => {
						if(accountId == null)
							return;
						
						PositionsDB.GetPosition(accountId, position => {
							if(position == null)
								position = new PlayerPosition();
							
							LocationsDB.GetLocation(accountId, location => {
								if(location == null)
									return;
								
								// TODO: This is not 100% correct as it might get overwritten by the server
								PositionsDB.SetPosition(player.accountId, position);
								
								player.location = location;
							});
						});
					});
				}else if(msg.StartsWith("//create ") && player.accessLevel >= AccessLevel.GameMaster) {
					var param = msg.Substring("//create ".Length);
					var spacePos = param.IndexOf(' ');
					var serverTypeString = param.Substring(0, spacePos);
					var mapName = param.Substring(spacePos + 1);
					
					switch(serverTypeString.ToLower()) {
						case "ffa":
							new LobbyFFA(mapName).Register();
							break;
						
						case "town":
							new LobbyTown(mapName).Register();
							break;
						
						/*case "match":
							new LobbyMatch().Register();
							break;*/
					}
				} else if(msg.StartsWith("//ginvite ")) {
					/*StartCoroutine(lobbyGameDB.GetAccountIdByPlayerName(msg.Split(' ')[1], data => {
						Debug.Log ("ginvite: " + data);
					}));*/
				} else {
					return false;
				}
				
				return true;
		}
	}
	
	// GetServerType
	public static ServerType GetServerType(string serverTypeString) {
		switch(serverTypeString.ToLower()) {
			case "ffa": return ServerType.FFA;
			case "town": return ServerType.Town;
			case "arena": return ServerType.Arena;
			default: return ServerType.World;
		}
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void ClientChat(string channelName, string msg, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		// Command?
		if(ProcessLobbyChatCommands(player, msg)) {
			LogManager.Chat.Log("Lobby chat command: [" + channelName + "][" + player.name + "] '" + msg + "'");
			return;
		}
		
		// Add instance to channel name
		if(channelName == "Map") {
			if(player.canUseMapChat) {
				var instance = player.instance;
				
				if(instance == null) {
					LogManager.Chat.LogError("Player instance is null on [" + channelName + "][" + player.name + "] '" + msg + "'");
					return;
				}
				
				var postfix = instance.node.publicAddress + ":" + instance.port;
				channelName += "@" + postfix;
			} else {
				LogManager.Chat.LogError("Player tries to use map chat while not being in an instance [" + channelName + "][" + player.name + "] '" + msg + "'");
				LogManager.Chat.LogError(player.gameInstance.ToString());
				return;
			}
		}
		
		// Log all chat tries
		LogManager.Chat.Log("[" + channelName + "][" + player.name + "] '" + msg + "'");
		
		// Access level?
		if(channelName == "Announcement" && player.accessLevel < AccessLevel.CommunityManager) {
			LogManager.Chat.LogError("Player tried to chat in announcement channel without having the rights for it!");
			return;
		}
		
		// Does the channel exist?
		if(!LobbyChatChannel.channels.ContainsKey(channelName)) {
			LogManager.Chat.LogError(string.Format("Channel '{0}' does not exist in the global channel list!", channelName));
			return;
		}
		
		var channel = LobbyChatChannel.channels[channelName];
		
		// Channel member?
		if(!channel.members.Contains(player)) {
			LogManager.Chat.LogError(string.Format("Player '{0}' is not a member of chat channel '{1}'!", player.name, channelName));
			return;
		}
		
		// Broadcast message
		channel.BroadcastMessage(player.name, msg);
	}
}
