using uLobby;
using System.Collections.Generic;

public class LobbyPlayer {
	public static Dictionary<string, LobbyPlayer> accountIdToLobbyPlayer = new Dictionary<string, LobbyPlayer>();
	public static Dictionary<LobbyPeer, LobbyPlayer> peerToLobbyPlayer = new Dictionary<LobbyPeer, LobbyPlayer>();
	public static List<LobbyPlayer> list = new List<LobbyPlayer>();
	
	public LobbyPeer peer;
	public PlayerAccount account;

	private AsyncProperty<string>.ConnectCallBack sendPlayerName;

	// Constructor
	public LobbyPlayer(Account uLobbyAccount) {
		account = PlayerAccount.Get(uLobbyAccount.id.value);
		peer = AccountManager.Master.GetLoggedInPeer(uLobbyAccount);

		sendPlayerName = data => {
			this.RPC("ReceiveAccountInfo", account.id, "playerName", data);
		};

		account.playerName.onValueChange += sendPlayerName;
		
		account.onlineStatus.value = OnlineStatus.Online;

		account.friendsList.Get(data => {
			foreach(var friend in data.allFriends) {
				LogManager.General.Log("Subscribing to " + friend);
				var friendAccount = friend.account;

				friendAccount.onlineStatus.Connect(status => {
					LogManager.General.Log("Sending online status of " + friendAccount + " to " + this);
					this.RPC("ReceiveAccountInfo", friendAccount.id, "onlineStatus", status.GetType().FullName, Jboy.Json.WriteObject(status));
				});
			}
		});

		LobbyPlayer.list.Add(this);
		LobbyPlayer.accountIdToLobbyPlayer[account.id] = this;
		LobbyPlayer.peerToLobbyPlayer[peer] = this;
	}

	// Get
	public static LobbyPlayer Get(LobbyMessageInfo info) {
		return LobbyPlayer.Get(info.sender);
	}

	// Get
	public static LobbyPlayer Get(LobbyPeer peer) {
		LobbyPlayer player;
		
		// Load from cache or create new player
		if(!LobbyPlayer.peerToLobbyPlayer.TryGetValue(peer, out player)) {
			var uLobbyAccount = AccountManager.Master.GetLoggedInAccount(peer);
			player = new LobbyPlayer(uLobbyAccount);
		}
		
		return player;
	}

	// RPC
	public void RPC(string rpcName, params object[] args) {
		Lobby.RPC(rpcName, peer, args);
	}

	// Removes a player - This function can be called from logout and disconnect!
	public void Remove() {
		// Remove the player from the queue he was in
		//if(queue != null)
		//	queue.RemovePlayer(this);
		
		// Remove game instance associations
		//gameInstance = null;
		
		// Broadcast offline status
		//onlineStatus = OnlineStatus.Offline;
		
		// Remove the reference from the dictionary
		LobbyPlayer.accountIdToLobbyPlayer.Remove(account.id);

		// Remove player from peer list
		LobbyPlayer.peerToLobbyPlayer.Remove(peer);
		
		// Remove the player from the global list
		LobbyPlayer.list.Remove(this);
		
		// Remove the player from all chat channels.
		// The list is copied because channels could be deleted after removing players.
		//foreach(var channel in new List<LobbyChatChannel>(channels)) {
		//	channel.RemovePlayer(this);
		//}

		// Offline status
		account.onlineStatus.value = OnlineStatus.Offline;

		// Remove event listeners
		account.playerName.onValueChange -= sendPlayerName;
		
		// Treat him as if he is disconnected for existing objects
		peer = null;
	}

	// ToString
	public override string ToString() {
		return name;
	}

#region Properties
	// Name
	public string name {
		get {
			return account.playerName.value;
		}
	}

	// E-Mail
	public string email {
		get {
			return account.email.value;
		}
	}

	// Friends
	public FriendsList friends {
		get {
			return account.friendsList.value;
		}
	}

	// IP
	public string ip {
		get {
			return peer.endpoint.Address.ToString();
		}
	}

	// Disconnected
	public bool disconnected {
		get {
			if(peer == null)
				return true;
			
			return peer.type == LobbyPeerType.Disconnected;
		}
	}
#endregion
}
