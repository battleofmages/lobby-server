using uLobby;
using System.Collections;
using System.Collections.Generic;

public class InMemoryFriendManager : IFriendOperations {
	private Dictionary<AccountID, FriendListRecord> friendsLists = new Dictionary<AccountID, FriendListRecord>();
	
	// GetFriendListRecordCoroutine
	IEnumerator IFriendOperations.GetFriendListRecordCoroutine(AccountID accountID, Request<FriendListRecord> request) {
		if (!friendsLists.ContainsKey(accountID))
		{
			friendsLists[accountID] = StorageLayerUtility.FriendListRecordUtility.CreateFriendListRecord();
		}
		StorageLayerUtility.RequestUtility.SetResult(request,friendsLists[accountID]);
		yield break;
	}
	
	// SetFriendListRecordCoroutine
	IEnumerator IFriendOperations.SetFriendListRecordCoroutine(AccountID accountID, FriendListRecord record, Request request) {
		friendsLists[accountID] = record;
		yield break;
	}
}