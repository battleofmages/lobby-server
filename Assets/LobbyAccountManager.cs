using uLobby;
using UnityEngine;
using System.Collections;

public class LobbyAccountManager : MonoBehaviour {
	// Start
	void Start() {
		// Make this class listen to Lobby events
		Lobby.AddListener(this);
	}
	
	// Sends account activation mail
	void SendActivationMail(string email, string token) {
		LogManager.General.Log("Sending activation mail to '" + email + "'...");
		
		var emailToken = System.Uri.EscapeDataString(token);
		Mail.Send(email, "Activate your Battle of Mages account", "Click this link to activate your account:\nhttp://battleofmages.com/scripts/activate.php?email=" + email + "&token=" + emailToken);
	}
	
#region RPCs
	// --------------------------------------------------------------------------------
	// Account Management RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	IEnumerator LobbyRegisterAccount(string email, string password, LobbyMessageInfo info) {
		// Validate data
		// Password is modified at this point anyway, no need to check it
		if(!Validator.email.IsMatch(email) && !GameDB.IsTestAccount(email))
			yield break;
		
		// Check if email has already been registered
		bool emailExists = false;
		
		yield return LobbyGameDB.GetAccountIdByEmail(email, data => {
			if(data != null) {
				emailExists = true;
			}
		});
		
		if(emailExists) {
			Lobby.RPC("EmailAlreadyExists", info.sender);
			yield break;
		}
		
		// Register account in uLobby
		var registerReq = AccountManager.Master.RegisterAccount(email, password);
		yield return registerReq.WaitUntilDone();
		
		// Bug in uLobby: We need to call this explicitly on the client
		if(!registerReq.isSuccessful) {
			var exception = (AccountException)registerReq.exception;
			var error = exception.error;
			
			Lobby.RPC("_RPCOnRegisterAccountFailed", info.sender, email, error);
			yield break;
		}
		
		// Set email for the account
		Account account = registerReq.result;
		yield return LobbyGameDB.SetEmail(account.id.value, email, data => {
			// ...
		});
		
		// Bug in uLobby: We need to call this explicitly on the client
		Lobby.RPC("_RPCOnAccountRegistered", info.sender, account);
		
		// Log it
		LogManager.General.Log("New account has been registered: E-Mail: '" + email + "' IP: " + info.sender.ToString().Split(',')[2]);
		
		// Activation mail
		if(!GameDB.IsTestAccount(email)) {
			LobbyGameDB.PutAccountAwaitingActivation(
				email,
				(data) => {
					SendActivationMail(email, data);
				}
			);
		}
	}
	
	[RPC]
	IEnumerator LobbyAccountLogIn(string email, string password, string deviceId, LobbyMessageInfo info) {
		// Check account activation
		bool activated = false;
		
		if(!GameDB.IsTestAccount(email)) {
			yield return LobbyGameDB.GetAccountAwaitingActivation(
				email,
				(data) => {
					if(string.IsNullOrEmpty(data))
						activated = true;
					else
						activated = false;
				}
			);
		} else {
			activated = true;
		}
		
		if(!activated) {
			Lobby.RPC("AccountNotActivated", info.sender, email);
			yield break;
		}
		
		// TODO: Check device ID
		
		// Login
		LogManager.General.Log("Login attempt: '" + email + "' on device " + deviceId);
		
		// Get account
		var getAccountReq = AccountManager.Master.TryGetAccount(email);
		yield return getAccountReq.WaitUntilDone();
		
		if(getAccountReq.isSuccessful) {
			var account = getAccountReq.result;
			
			// Log out account if logged in from a different peer
			if(account != null && AccountManager.Master.IsLoggedIn(account)) {
				var peer = AccountManager.Master.GetLoggedInPeer(account);
				
				LogManager.General.LogWarning(string.Format(
					"Account '{0}' already logged in, kicking old peer: {1}",
					account.name,
					peer
					));
				
				var logoutReq = AccountManager.Master.LogOut(peer);
				yield return logoutReq.WaitUntilDone();
			}
		}
		
		// Login
		var loginReq = AccountManager.Master.LogIn(info.sender, email, password);
		yield return loginReq.WaitUntilDone();
		
		if(!loginReq.isSuccessful) {
			var exception = (AccountException)loginReq.exception;
			var error = exception.error;
			
			// Bug in uLobby: We need to call this explicitly on the client
			Lobby.RPC("_RPCOnLogInFailed", info.sender, email, error);
			yield break;
		}
	}
	
	[RPC]
	IEnumerator LobbyAccountLogOut(LobbyMessageInfo info) {
		var req = AccountManager.Master.LogOut(info.sender);
		yield return req.WaitUntilDone();
	}
	
	[RPC]
	void ResendActivationMail(string email, LobbyMessageInfo info) {
		LobbyGameDB.GetAccountAwaitingActivation(
			email,
			(token) => {
				if(!string.IsNullOrEmpty(token)) {
					SendActivationMail(email, token);
					Lobby.RPC("ActivationMailSent", info.sender);
				}
			}
		);
	}
	
	[RPC]
	IEnumerator ChangePassword(string newPassword, LobbyMessageInfo info) {
		// Get the account
		var player = LobbyServer.GetLobbyPlayer(info);
		
		// Change name
		LogManager.General.Log("Player '" + player.name + "' has requested to change its password.");
		yield return StartCoroutine(LobbyGameDB.SetPassword(player, newPassword));
	}
#endregion
}
