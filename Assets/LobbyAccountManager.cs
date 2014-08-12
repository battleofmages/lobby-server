using uLobby;
using uGameDB;
using UnityEngine;
using System.Collections;
using System.Linq;

public class LobbyAccountManager : MonoBehaviour {
	// Start
	void Start() {
		// Make this class listen to Lobby events
		Lobby.AddListener(this);
	}
	
	// Sends account activation mail
	Coroutine SendActivationMail(string email, string token) {
		return StartCoroutine(SendActivationMailCoroutine(email, token));
	}
	
	// SendActivationMailCoroutine
	IEnumerator SendActivationMailCoroutine(string email, string token) {
		LogManager.General.Log("Sending activation mail to '" + email + "'...");
		
		var emailToken = System.Uri.EscapeDataString(token);
		var sendMailRequest = new WWW("https://battleofmages.com/scripts/send-activation-mail.php?email=" + email + "&token=" + emailToken);
		yield return sendMailRequest;
		
		if(sendMailRequest.error == null) {
			LogManager.General.Log("Sent activation mail to '" + email + "'");
		} else {
			LogManager.General.LogError("Failed sending activation mail to '" + email + "'");
		}
	}
	
	// DeleteAccount
	// TODO: This is incomplete and should only be used for not yet activated accounts
	public static IEnumerator DeleteAccount(string accountId) {
		string email = null;
		
		yield return LobbyGameDB.GetEmail(
			accountId,
			data => {
				if(data != null)
					email = data;
			}
		);
		
		if(string.IsNullOrEmpty(email))
			yield break;
		
		// Get a list of all buckets
		var getBucketsReq = Bucket.GetBuckets();
		yield return getBucketsReq.WaitUntilDone();
		
		if(getBucketsReq.isSuccessful) {
			foreach(var bucket in getBucketsReq.GetBucketEnumerable()) {
				if(bucket.name.StartsWith("AccountTo")) {
					yield return bucket.Remove(accountId).WaitUntilDone();
				}
			}
		}
		
		var emailBucketNames = new string[] {
			"AccountsAwaitingActivation",
			"uLobby AccountNameToID"
		};
		
		foreach(var bucketName in emailBucketNames) {
			yield return new Bucket(bucketName).Remove(email).WaitUntilDone();
		}
		
		yield return new Bucket("uLobby Accounts").Remove(email).WaitUntilDone();
	}
	
	// DeleteAllUnactivatedAccounts
	public static IEnumerator DeleteAllUnactivatedAccounts() {
		var emailToIdBucket = new Bucket("uLobby AccountNameToID");
		
		var request = new Bucket("AccountsAwaitingActivation").GetKeys();
		yield return request.WaitUntilDone();
		
		if(request.hasFailed)
			yield break;
		
		foreach(var email in request.GetKeyEnumerable()) {
			emailToIdBucket.Get(email, Constants.Replication.Default, (req) => {
				var accountId = req.GetValue<string>();
				LogManager.General.Log("Deleting account '" + req.key + "' with ID '" + accountId + "'");
				GameDB.instance.StartCoroutine(DeleteAccount(accountId));
			}, null);
			//yield return req.WaitUntilDone();
			
		}
	}
	
	// CreateAccount
	public static IEnumerator CreateAccount(string accountId, string email, string password) {
		// Test accounts
		if(GameDB.IsTestAccount(email)) {
			password = email;
		}
		
		// Log
		LogManager.General.Log(accountId + ", " + email + ", " + password);
		
		// SHA 512 encryption
		password = GameDB.EncryptPasswordString(password);
		
		// AccountNameToID
		yield return GameDB.instance.StartCoroutine(GameDB.Set<string>(
			"AccountNameToID",
			email,
			accountId,
			null
		));
		
		// Create password hash
		LogManager.General.Log("Generating password hash");
		var saltedPasswordHash = SaltedPasswordHash.GenerateSaltedPasswordHash(SaltedPasswordHash.GeneratePasswordHash(password + email));
		
		// Create game account
		var gameAccount = new GameAccount(
			accountId,
			email,
			saltedPasswordHash.passwordHash,
			saltedPasswordHash.salt
		);
		
		// Save game account
		LogManager.General.Log("Saving game account: " + accountId);
		yield return GameDB.instance.StartCoroutine(GameDB.Set<GameAccount>(
			"Accounts",
			accountId,
			gameAccount,
			null
		));
	}
	
	// CopyULobbyAccounts
	public static IEnumerator CopyULobbyAccounts() {
		var emailToIdBucket = new Bucket("uLobby AccountNameToID");
		
		var request = emailToIdBucket.GetKeys();
		yield return request.WaitUntilDone();
		
		if(request.hasFailed)
			yield break;
		
		foreach(var email in request.GetKeyEnumerable()) {
			emailToIdBucket.Get(email, Constants.Replication.Default, (req) => {
				var accountId = req.GetValue<string>();
				LogManager.General.Log("Copying account '" + req.key + "' with ID '" + accountId + "'");
				GameDB.instance.StartCoroutine(
					CreateAccount(
						accountId,
						req.key,
						GameDB.GetRandomString(10)
					)
				);
			}, null);
		}
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
		LogManager.General.Log(string.Format(
			"[{0}] New account has been registered: E-Mail: '{1}'",
			info.sender.endpoint.Address,
			email
		));
		
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
