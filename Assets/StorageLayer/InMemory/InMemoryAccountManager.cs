using uLobby;
using System.Collections;
using System.Collections.Generic;

public class InMemoryAccountManager : IAccountOperations {
	public List<AccountRecord> accounts = new List<AccountRecord>();
	
	private const int _AccountIDLengthInBytes = 6;
	private const int _MaxAccountIDGenerationAttempts = 10;
	
	// AddAccountCoroutine
	IEnumerator IAccountOperations.AddAccountCoroutine(AccountRecord account, Request<AccountRecord> request) {
		accounts.Add(account);
		StorageLayerUtility.RequestUtility.SetResult(request,account);
		yield break;
	}
	
	// GetAccountCoroutine
	IEnumerator IAccountOperations.GetAccountCoroutine(AccountID accountID, string accountName, bool exceptionIfInvalid, Request<Account> request) {
		foreach (var account in accounts) {
			if (account.id == accountID || (accountID == null && accountName == account.name)) {
				StorageLayerUtility.RequestUtility.SetResult(request,StorageLayerUtility.CreateAccount(account));
				yield break;
			}
		}
		
		StorageLayerUtility.RequestUtility.SetResult(request,null);
		if(exceptionIfInvalid) {
			StorageLayerUtility.RequestUtility.ThrowException(request,(StorageLayerUtility.Exceptions.CreateAccountException("Account with id " + accountID + " and name " + accountName + " does not exist.")));
		}
		
		yield break;
	}
	
	// GetAccountRecordCoroutine
	IEnumerator IAccountOperations.GetAccountRecordCoroutine(string name, Request<AccountRecord> request) {
		foreach (var account in accounts)
		{
			if (account.name == name)
			{
				StorageLayerUtility.RequestUtility.SetResult(request,account);
				yield break;
			}
		}
		StorageLayerUtility.RequestUtility.SetResult(request,null);
		yield break;
		
	}
	
	// GetAccountRecordCoroutine
	IEnumerator IAccountOperations.GetAccountRecordCoroutine(AccountID accountID, Request<AccountRecord> request) {
		foreach (var account in accounts) {
			if(account.id == accountID) {
				StorageLayerUtility.RequestUtility.SetResult(request,account);
				yield break;
			}
		}
		
		StorageLayerUtility.RequestUtility.SetResult(request,null);
		yield break;
	}
	
	// GetNewAccountIDCoroutine
	IEnumerator IAccountOperations.GetNewAccountIDCoroutine(Request<AccountID> request) {
		for (int i = 0; i < _MaxAccountIDGenerationAttempts; ++i) {
			AccountID randomID = new AccountID(GetRandomIdentifier(_AccountIDLengthInBytes));
			
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
	
	// GetRandomIdentifier
	internal static string GetRandomIdentifier(int lengthInBytes) {
		byte[] array = new byte[lengthInBytes];
		System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes(array);
		
		return System.Convert.ToBase64String(array);
	}
	
	// UpdateAccountCoroutine
	IEnumerator IAccountOperations.UpdateAccountCoroutine(IAccount account, AccountUpdate update, Request<Account> request) {
		Request<AccountRecord> getAccountRequest;
		getAccountRequest = StorageLayerUtility.GetAccountRecord(account.id);
		yield return getAccountRequest.WaitUntilDone(); if (StorageLayerUtility.RequestUtility.PropagateException(getAccountRequest,request)) yield break;
		AccountRecord record = getAccountRequest.result;
		
		if(StorageLayerUtility.AccountUpdateUtility.isPasswordChanged(update))
			record.passwordHash = SaltedPasswordHash.GenerateSaltedPasswordHash(SaltedPasswordHash.GeneratePasswordHash(StorageLayerUtility.AccountUpdateUtility.GetPassword(update) + record.name));
		
		if(StorageLayerUtility.AccountUpdateUtility.IsDataChanged(update))
			record.data = StorageLayerUtility.AccountUpdateUtility.GetData(update);
		
		Account updatedAccount = StorageLayerUtility.CreateAccount(record);
		StorageLayerUtility.RequestUtility.SetResult(request,updatedAccount);
		yield break;
	}
	
	// GetAccountWithID
	private IEnumerator GetAccountWithID(AccountID id, Request<AccountRecord> request) {
		AccountRecord record = null;
		
		foreach (var account in accounts) {
			if (account.id == id) {
				record = account;
				break;
			}
		}
		
		if(record == null) {
			StorageLayerUtility.RequestUtility.ThrowException(request,StorageLayerUtility.Exceptions.CreateAccountException("The account doesn't exist."));
		} else {
			StorageLayerUtility.RequestUtility.SetResult(request,record);
		}
		
		yield break;
	}
}

