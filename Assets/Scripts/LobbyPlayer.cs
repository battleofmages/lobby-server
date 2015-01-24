using uLobby;
using System.Collections.Generic;
using System.Linq;

public class LobbyPlayer {
	public static Dictionary<string, LobbyPlayer> accountIdToLobbyPlayer = new Dictionary<string, LobbyPlayer>();
	public static Dictionary<LobbyPeer, LobbyPlayer> peerToLobbyPlayer = new Dictionary<LobbyPeer, LobbyPlayer>();
	public static List<LobbyPlayer> list = new List<LobbyPlayer>();
	
	public LobbyPeer peer;
	public PlayerAccount account;

	// Constructor
	public LobbyPlayer(Account uLobbyAccount) {
		account = PlayerAccount.Get(uLobbyAccount.id.value);
		peer = AccountManager.Master.GetLoggedInPeer(uLobbyAccount);

		LogManager.General.Log("New player: " + this);

		LobbyPlayer.list.Add(this);
		LobbyPlayer.accountIdToLobbyPlayer[account.id] = this;
		LobbyPlayer.peerToLobbyPlayer[peer] = this;

		// Name
		account.playerName.Connect(this, data => {
			this.RPC("ReceiveAccountInfo", account.id, "playerName", data.GetType().FullName, Jboy.Json.WriteObject(data));
		});

		// Friends list
		account.friendsList.Connect(this, friendsList => {
			// Subscribe to online status
			foreach(var friend in friendsList.allFriends) {
				SubscribeToOnlineStatus(friend.account);
			}

			// Send friends list to owner
			this.RPC("ReceiveAccountInfo", account.id, "friendsList", friendsList.GetType().FullName, Jboy.Json.WriteObject(friendsList));
		});

		// Online status
		account.onlineStatus.value = OnlineStatus.Online;
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
		//LogManager.General.Log(peer + " > " + rpcName + "(" + string.Join(", ", args.Select(x => x.ToString()).ToArray()) + ")");
		Lobby.RPC(rpcName, peer, args);
	}

	// SubscribeToOnlineStatus
	public void SubscribeToOnlineStatus(Friend friend) {
		SubscribeToOnlineStatus(friend.account);
	}

	// SubscribeToOnlineStatus
	public void SubscribeToOnlineStatus(PlayerAccount friendAccount) {
		// Disconnect old listeners
		friendAccount.onlineStatus.Disconnect(this);

		// Set new listener
		friendAccount.onlineStatus.Connect(this, status => {
			this.RPC("ReceiveAccountInfo", friendAccount.id, "onlineStatus", status.GetType().FullName, Jboy.Json.WriteObject(status));
		});
	}

	// Removes a player - This function can be called from logout and disconnect!
	public void Remove() {
		LogManager.General.Log("Remove player: " + this);

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

		// Remove event listeners
		account.friendsList.Get(data => {
			foreach(var friend in data.allFriends) {
				friend.account.onlineStatus.Disconnect(this);
			}
		});

		// Remove event listeners
		account.playerName.Disconnect(this);
		account.friendsList.Disconnect(this);

		// Offline status
		account.onlineStatus.value = OnlineStatus.Offline;
		
		// Treat him as if he is disconnected for existing objects
		peer = null;
	}

	// ToString
	public override string ToString() {
		return account.ToString();
	}

#region Properties
	// Name
	public string name {
		get {
			if(account.playerName.available)
				return account.playerName.value;

			return "";
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
