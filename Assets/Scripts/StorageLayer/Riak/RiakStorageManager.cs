using uLobby;

public class RiakStorageManager : IStorageManager {
	// Initialize
	void IStorageManager.Initialize() {
		
	}
	
	// Is ready to use
	bool IStorageManager.isReadyToUse {
		get {
			return uGameDB.Database.isConnected;
		}
	}
}
