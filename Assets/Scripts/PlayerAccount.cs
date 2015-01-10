using uLobby;
using System.Collections.Generic;

public class PlayerAccount : PlayerAccountBase, AsyncRequester {
	public static Dictionary<string, PlayerAccount> idToAccount = new Dictionary<string, PlayerAccount>();
	
	private Dictionary<string, CallBack> propertyGetters;
	private Dictionary<string, WriteCallBack> propertySetters;

	// Delegate
	private delegate void WriteCallBack(object val, WriteAsyncPropertyCallBack callBack);

	// Private constructor
	private PlayerAccount() {
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
				"friendsList",
				() => {
					FriendsDB.GetFriends(id, data => {
						friendsList.value = data;
					});
				}
			}
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
			}
		};
	}

	// Get
	public static PlayerAccount Get(string accountId) {
		PlayerAccount acc;
		
		// Load from cache or create new account
		if(!PlayerAccount.idToAccount.TryGetValue(accountId, out acc)) {
			acc = new PlayerAccount {
				id = accountId
			};
			
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
		return string.Format("[PlayerAccount: {0}]", id);
	}
}
