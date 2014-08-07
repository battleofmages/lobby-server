using UnityEngine;
using uLobby;

public class ChatServer : MonoBehaviour {
	// Commands
	protected ChatCommand<LobbyPlayer>[] playerCommands;
	protected ChatCommand<LobbyPlayer>[] vipCommands;
	protected ChatCommand<LobbyPlayer>[] communityManagerCommands;
	protected ChatCommand<LobbyPlayer>[] gameMasterCommands;
	protected ChatCommand<LobbyPlayer>[] adminCommands;
	
	// Start
	void Start () {
		// Player
		playerCommands = new ChatCommand<LobbyPlayer>[]{
			// practice
			new ChatCommand<LobbyPlayer>(
				@"^practice$",
				(player, args) => {
					if(!player.inMatch) {
						LobbyQueue.CreatePracticeMatch(player);
					} else {
						// Notify player ...
					}
				}
			),
			
			// online
			new ChatCommand<LobbyPlayer>(
				@"^online$",
				(player, args) => {
					LobbyServer.SendSystemMessage(player, "Players online: " + LobbyPlayer.list.Count);
				}
			)
		};
		
		// VIP
		vipCommands = new ChatCommand<LobbyPlayer>[]{
			// list
			new ChatCommand<LobbyPlayer>(
				@"^list$",
				(player, args) => {
					LobbyServer.SendSystemMessage(player, "Town: " + LobbyTown.running.Count);
					LobbyServer.SendSystemMessage(player, "World: " + LobbyWorld.running.Count);
					LobbyServer.SendSystemMessage(player, "Arena: " + LobbyMatch.running.Count);
					LobbyServer.SendSystemMessage(player, "FFA: " + LobbyFFA.running.Count);
				}
			)
		};
		
		// Community Manager
		communityManagerCommands = new ChatCommand<LobbyPlayer>[]{
			// goto
			new ChatCommand<LobbyPlayer>(
				@"^goto ([^ ]+) (.*)$",
				(player, args) => {
					var serverType = ChatServer.GetServerType(args[0]);
					var mapName = args[1];
					
					player.location = new PlayerLocation(mapName, serverType);
				}
			),
			
			// moveToPlayer
			new ChatCommand<LobbyPlayer>(
				@"^moveToPlayer (.*)$",
				(player, args) => {
					var playerName = args[1];
					
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
				}
			),
		};
		
		// Game Master
		gameMasterCommands = new ChatCommand<LobbyPlayer>[]{
			// start
			new ChatCommand<LobbyPlayer>(
				@"^start ([^ ]+) (.*)$",
				(player, args) => {
					var serverType = ChatServer.GetServerType(args[0]);
					var mapName = args[1];
					
					switch(serverType) {
						case ServerType.FFA:
							new LobbyFFA(mapName).Register();
							break;
							
						case ServerType.Town:
							new LobbyTown(mapName).Register();
							break;
					}
				}
			),
		};
		
		// Admin
		adminCommands = new ChatCommand<LobbyPlayer>[]{
			
		};
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// ProcessChatCommands
	protected bool ProcessChatCommands(LobbyPlayer player, string msg, ChatCommand<LobbyPlayer>[] cmdList) {
		if(cmdList == null)
			return false;
		
		foreach(var cmd in cmdList) {
			if(cmd.Process(player, msg))
				return true;
		}
		
		return false;
	}
	
	// User and admin commands
	bool ProcessLobbyChatCommands(LobbyPlayer player, string msg) {
		msg = msg.ReplaceCommands();
		
		if(!msg.StartsWith("//"))
			return false;
		
		// Remove the 
		msg = msg.Substring(2);
		
		if(player.accessLevel >= AccessLevel.Player && ProcessChatCommands(player, msg, playerCommands))
			return true;
		
		if(player.accessLevel >= AccessLevel.VIP && ProcessChatCommands(player, msg, vipCommands))
			return true;
		
		if(player.accessLevel >= AccessLevel.CommunityManager && ProcessChatCommands(player, msg, communityManagerCommands))
			return true;
		
		if(player.accessLevel >= AccessLevel.GameMaster && ProcessChatCommands(player, msg, gameMasterCommands))
			return true;
		
		if(player.accessLevel >= AccessLevel.Admin && ProcessChatCommands(player, msg, adminCommands))
			return true;
		
		return false;
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
