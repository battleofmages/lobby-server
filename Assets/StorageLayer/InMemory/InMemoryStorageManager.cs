using uLobby;

public class InMemoryStorageManager : IStorageManager {
	// Initialize
	void IStorageManager.Initialize() {
		
	}
	
	// Is ready to use
	bool IStorageManager.isReadyToUse {
		get {
			return true;
		}
	}
}
