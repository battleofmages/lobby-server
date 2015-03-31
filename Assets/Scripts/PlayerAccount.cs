using System.Linq;
using System.Collections.Generic;
using BoM;
using BoM.Async;
using BoM.Friends;

public class PlayerAccount : PlayerAccountBase, AsyncRequester {
	public static Dictionary<string, PlayerAccount> idToAccount = new Dictionary<string, PlayerAccount>();

	private Dictionary<string, DBProperty> properties = new Dictionary<string, DBProperty>();

	// Private constructor
	private PlayerAccount(string accountId) {
		id = accountId;

		// Async properties
		base.Init(this);
		
		// Player name
		AddProperty(
			"playerName",

			// Get
			() => {
				LobbyGameDB.GetPlayerName(id, data => {
					if(data == null)
						data = "";

					playerName.directValue = data;
				});
			},
			
			// Set
			(val, callBack) => {
				LobbyGameDB.SetPlayerName(id, (string)val, (data) => {
					callBack(data);
				});
			}
		);

		// E-Mail
		AddProperty(
			"email",
			
			// Get
			() => {
				LobbyGameDB.GetEmail(id, data => {
					if(data == null)
						data = "";
					
					email.directValue = data;
				});
			},
		
			// Set
			(val, callBack) => {
				LobbyGameDB.SetEmail(id, (string)val, (data) => {
					callBack(data);
				});
			}
		);

		// Avatar URL
		AddProperty(
			"avatarURL",

			// Get
			() => {
				email.Get(data => {
					avatarURL.directValue = "https://www.gravatar.com/avatar/" + GameDB.MD5(data.Trim().ToLower());
				});
			}
		);

		// Friends list
		AddProperty(
			"friendsList",
				
			// Get
			() => {
				FriendsDB.GetFriends(id, data => {
					if(data == null)
						data = new FriendsList();
					
					friendsList.directValue = data;
				});
			},
			
			// Set
			(val, callBack) => {
				FriendsDB.SetFriends(id, (FriendsList)val, (data) => {
					callBack(data);
				});
			}
		);

		// Party
		AddProperty(
			"party",
			
			// Get
			() => {
				if(party.value == null) {
					var newParty = new Party();
					newParty.accountIds.Add(id);
					
					party.directValue = newParty;
				}
			}
		);

		// Online status
		AddProperty(
			"onlineStatus",
			
			// Get
			() => {
				onlineStatus.directValue = onlineStatus.value;
			}
		);

		// Country
		AddProperty (
			"country", 

			// Get
			() => {
				country.directValue = "NO";
			}
		);

		// Country
		AddProperty (
			"followers", 
			
			// Get
			() => {
				friendsList.Get(
					data => {
						FriendsDB.GetFollowers(id, allFollowers => {
							if(allFollowers == null) {
								followers.directValue = new List<string>(0);
							} else {
								// Remove people who are already in the friends list
								followers.directValue = allFollowers.Except(data.allFriendIds).ToList();
							}
						});
					}
				);
			}
		);
		AddProperty(
			"traits",

			// Get
			() => {
				TraitsDB.GetTraits(id, data => {
					if(data == null)
						data = new Traits();

					traits.directValue = data;
				});
			},

			// Set
			(val, callBack) => {
				TraitsDB.SetTraits(id, (Traits)val, (data) => {
					callBack(data);
				});
			}
		);
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

	// Delegate
	private delegate void WriteCallBack(object val, WriteAsyncPropertyCallBack callBack);

	// Property
	private class DBProperty {
		// Constructor
		public DBProperty(CallBack getter, WriteCallBack setter) {
			get = getter;
			set = setter;
		}
		
		public WriteCallBack set;
		public CallBack get;
	}
	
	// AddProperty
	private void AddProperty(string name, CallBack getter, WriteCallBack setter = null) {
		// Default setter
		if(setter == null) {
			setter = (val, callBack) => {
				callBack(val);
			};
		}

		properties[name] = new DBProperty(getter, setter);
	}

	// RequestAsyncProperty
	public void RequestAsyncProperty(string propertyName) {
		properties[propertyName].get();
	}

	// WriteAsyncProperty
	public void WriteAsyncProperty(string propertyName, object val, WriteAsyncPropertyCallBack callBack) {
		properties[propertyName].set(val, callBack);
	}

	// ToString
	public override string ToString() {
		if(playerName.available)
			return string.Format("{0} ({1})", playerName.value, id);
		
		return string.Format("({0})", id);
	}
}
