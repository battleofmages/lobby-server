using uLobby;
using System.Collections.Generic;

public class PlayerAccount : PlayerAccountBase, AsyncRequester {
	public static Dictionary<string, PlayerAccount> idToAccount = new Dictionary<string, PlayerAccount>();
	
	private Dictionary<string, CallBack> propertyGetters;
	private Dictionary<string, WriteCallBack> propertySetters;

	// Delegate
	private delegate void WriteCallBack(object val, WriteAsyncPropertyCallBack callBack);

	// Private constructor
	private PlayerAccount(string accountId) {
		id = accountId;

		// Async properties
		base.Init(this);

		// Getters
		propertyGetters = new Dictionary<string, CallBack>() {
			{
				"playerName",
				() => {
					LobbyGameDB.GetPlayerName(id, data => {
						if(data == null)
							data = "";

						playerName.value = data;
					});
				}
			},
			{
				"email",
				() => {
					LobbyGameDB.GetEmail(id, data => {
						if(data == null)
							data = "";
						
						email.value = data;
					});
				}
			},
			{
				"avatarURL",
				() => {
					email.Get(data => {
						avatarURL.value = "https://www.gravatar.com/avatar/" + GameDB.MD5(data.Trim().ToLower());
					});
				}
			},
			{
				"friendsList",
				() => {
					FriendsDB.GetFriends(id, data => {
						if(data == null)
							data = new FriendsList();

						friendsList.value = data;
					});
				}
			},
			{
				"party",
				() => {
					if(party.value == null) {
						var newParty = new Party();
						newParty.accountIds.Add(id);

						party.value = newParty;
					}
				}
			},
			{
				"onlineStatus",
				() => {
					onlineStatus.value = onlineStatus.value;
				}
			}
		};

		// Default setter
		PlayerAccount.WriteCallBack defaultSetter = (val, callBack) => {
			callBack(val);
		};

		// Setters
		propertySetters = new Dictionary<string, WriteCallBack>() {
			{
				"playerName",
				(val, callBack) => {
					LobbyGameDB.SetPlayerName(id, (string)val, (data) => {
						callBack(data);
					});
				}
			},
			{
				"email",
				(val, callBack) => {
					LobbyGameDB.SetEmail(id, (string)val, (data) => {
						callBack(data);
					});
				}
			},
			{
				"friendsList",
				(val, callBack) => {
					FriendsDB.SetFriends(id, (FriendsList)val, (data) => {
						callBack(data);
					});
				}
			},
			{
				"party",
				defaultSetter
			},
			{
				"onlineStatus",
				defaultSetter
			},
			{
				"avatarURL",
				defaultSetter
			}
		};
	}

	// Get
	public static PlayerAccount Get(string accountId) {
		PlayerAccount acc;
		
		// Load from cache or create new account
		if(!PlayerAccount.idToAccount.TryGetValue(accountId, out acc)) {
			acc = new PlayerAccount(accountId);
			
			PlayerAccount.idToAccount[accountId] = acc;
		}
		
		return acc;
	}

	// RequestAsyncProperty
	public void RequestAsyncProperty(string propertyName) {
		propertyGetters[propertyName]();
	}

	// WriteAsyncProperty
	public void WriteAsyncProperty(string propertyName, object val, WriteAsyncPropertyCallBack callBack) {
		propertySetters[propertyName](val, callBack);
	}

	// ToString
	public override string ToString() {
		if(playerName.available)
			return string.Format("{0} ({1})", playerName.value, id);
		
		return string.Format("({0})", id);
	}
}
