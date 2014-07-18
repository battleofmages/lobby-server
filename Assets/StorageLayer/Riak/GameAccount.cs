using uLobby;

public class GameAccount : JsonSerializable<GameAccount> {
	public string id;
	public string name;
	public string passwordHash;
	public string salt;
	
	// Empty constructor
	public GameAccount() {
		
	}
	
	// Constructor
	public GameAccount(string accountId, string nName, byte[] nPasswordHash, byte[] nSalt) {
		id = accountId;
		name = nName;
		passwordHash = System.Convert.ToBase64String(nPasswordHash);
		salt = System.Convert.ToBase64String(nSalt);
	}
	
	// Constructor
	public GameAccount(AccountRecord account) {
		CopyAccountRecord(account);
	}
	
	// CopyAccountRecord
	public void CopyAccountRecord(AccountRecord account) {
		id = account.id.value;
		name = account.name;
		passwordHash = System.Convert.ToBase64String(account.passwordHash.passwordHash);
		salt = System.Convert.ToBase64String(account.passwordHash.salt);
	}
	
	// ToAccountRecord
	public AccountRecord ToAccountRecord() {
		return StorageLayerUtility.CreateAccountRecord(
			name,
			new SaltedPasswordHash(
				System.Convert.FromBase64String(passwordHash),
				System.Convert.FromBase64String(salt)
			),
			new AccountID(id),
			null
		);
	}
	
	// ToAccount
	public Account ToAccount() {
		return StorageLayerUtility.CreateAccount(ToAccountRecord());
	}
}