using uLobby;
using System.Collections;
using System.Collections.Generic;

public class RiakAccountManager : IAccountOperations {
	public List<AccountRecord> accounts = new List<AccountRecord>();
	
	private const int _AccountIDLengthInBytes = 6;
	private const int _MaxAccountIDGenerationAttempts = 10;
	
	// AddAccountCoroutine
	IEnumerator IAccountOperations.AddAccountCoroutine(AccountRecord account, Request<AccountRecord> request) {
		// Accounts
		yield return GameDB.Async(GameDB.Set<GameAccount>(
			"Accounts",
			account.id.value,
			new GameAccount(account),
			data => {
				if(data != null)
					StorageLayerUtility.RequestUtility.SetResult(request, account);
			}
		));
		
		// EmailToID
		yield return GameDB.Async(GameDB.Set<string>(
			"EmailToID",
			account.name,
			account.id.value,
			null
		));
	}
	
	// GetAccountCoroutine
	IEnumerator IAccountOperations.GetAccountCoroutine(AccountID accountID, string accountName, bool exceptionIfInvalid, Request<Account> request) {
		string gameAccountId = null;
		
		// Do we have an ID already?
		if(accountID != null) {
			gameAccountId = accountID.value;
		} else {
			// Retrieve account ID
			yield return GameDB.Async(GameDB.Get<string>(
				"EmailToID",
				accountName,
				data => {
					gameAccountId = data;
				}
			));
			
			if(gameAccountId == null) {
				StorageLayerUtility.RequestUtility.SetResult(request, null);
				
				if(exceptionIfInvalid)
					StorageLayerUtility.RequestUtility.ThrowException(request, (StorageLayerUtility.Exceptions.CreateAccountException("Account name " + accountName + " does not exist.")));
				
				yield break;
			}
		}
		
		// Retrieve game account
		yield return GameDB.Async(GameDB.Get<GameAccount>(
			"Accounts",
			gameAccountId,
			data => {
				if(data == null) {
					StorageLayerUtility.RequestUtility.SetResult(request, null);
					
					if(exceptionIfInvalid)
						StorageLayerUtility.RequestUtility.ThrowException(request, (StorageLayerUtility.Exceptions.CreateAccountException("Account with id " + gameAccountId + " and name " + accountName + " does not exist.")));
				} else {
					StorageLayerUtility.RequestUtility.SetResult(request, data.ToAccount());
				}
			}
		));
	}
	
	// GetAccountRecordCoroutine
	IEnumerator IAccountOperations.GetAccountRecordCoroutine(string accountName, Request<AccountRecord> request) {
		string gameAccountId = null;
		
		// Retrieve account ID
		yield return GameDB.Async(GameDB.Get<string>(
			"EmailToID",
			accountName,
			data => {
				gameAccountId = data;
			}
		));
		
		if(gameAccountId == null) {
			StorageLayerUtility.RequestUtility.SetResult(request, null);
			yield break;
		}
		
		// Retrieve game account
		yield return GameDB.Async(GameDB.Get<GameAccount>(
			"Accounts",
			gameAccountId,
			data => {
				if(data == null) {
					StorageLayerUtility.RequestUtility.SetResult(request, null);
				} else {
					StorageLayerUtility.RequestUtility.SetResult(request, data.ToAccountRecord());
				}
			}
		));
	}
	
	// GetAccountRecordCoroutine
	IEnumerator IAccountOperations.GetAccountRecordCoroutine(AccountID accountID, Request<AccountRecord> request) {
		// Retrieve game account
		yield return GameDB.Async(GameDB.Get<GameAccount>(
			"Accounts",
			accountID.value,
			data => {
				if(data == null) {
					StorageLayerUtility.RequestUtility.SetResult(request, null);
				} else {
					StorageLayerUtility.RequestUtility.SetResult(request, data.ToAccountRecord());
				}
			}
		));
	}
	
	// GetNewAccountIDCoroutine
	IEnumerator IAccountOperations.GetNewAccountIDCoroutine(Request<AccountID> request) {
		for (int i = 0; i < _MaxAccountIDGenerationAttempts; ++i) {
			AccountID randomID = new AccountID(GameDB.GetRandomString(_AccountIDLengthInBytes));
			
			Request<bool> accountExistsRequest = uLobby.AccountManager.Master.AccountExists(randomID);
			yield return accountExistsRequest.WaitUntilDone(); if (StorageLayerUtility.RequestUtility.PropagateException(accountExistsRequest,request)) yield break;
			
			if (!accountExistsRequest.result) {
				Log.Debug(LogFlags.Account, "Generated new account ID ", randomID.value, " after ", Quantify(i + 1, "attempt"));
				
				StorageLayerUtility.RequestUtility.SetResult(request,randomID);
				
				yield break;
			}
		}
		
		StorageLayerUtility.RequestUtility.ThrowException(request,StorageLayerUtility.Exceptions.CreateAccountException("Failed to generate account ID after " + _MaxAccountIDGenerationAttempts + " attempts"));
	}
	
	// Quantify
	internal static string Quantify(int quantity, string nounInSingular) {
		return quantity + " " + nounInSingular + (quantity != 1 ? "s" : "");
	}
	
	// UpdateAccountCoroutine
	IEnumerator IAccountOperations.UpdateAccountCoroutine(IAccount account, AccountUpdate update, Request<Account> request) {
		// Get account record
		Request<AccountRecord> getAccountRequest;
		getAccountRequest = StorageLayerUtility.GetAccountRecord(account.id);
		yield return getAccountRequest.WaitUntilDone();
		
		// Did it cause an exception?
		if(StorageLayerUtility.RequestUtility.PropagateException(getAccountRequest, request))
			yield break;
		
		AccountRecord record = getAccountRequest.result;
		
		// Password change
		if(StorageLayerUtility.AccountUpdateUtility.isPasswordChanged(update))
			record.passwordHash = SaltedPasswordHash.GenerateSaltedPasswordHash(SaltedPasswordHash.GeneratePasswordHash(StorageLayerUtility.AccountUpdateUtility.GetPassword(update) + record.name));
		
		Account updatedAccount = StorageLayerUtility.CreateAccount(record);
		
		// Save in accounts database
		yield return GameDB.Async(GameDB.Set<GameAccount>(
			"Accounts",
			updatedAccount.id.value,
			new GameAccount(record),
			data => {
				if(data != null)
					StorageLayerUtility.RequestUtility.SetResult(request, updatedAccount);
			}
		));
	}
}
