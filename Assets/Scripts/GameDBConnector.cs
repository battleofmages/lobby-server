using UnityEngine;
using System.IO;
using uGameDB;

public class GameDBConnector : SingletonMonoBehaviour<GameDBConnector>, Initializable {
	public string databaseConfigPath;
	public int socketPoolSize = Defaults.PoolSize;

	public event OnConnectCallback onConnect;
	public event OnConnectFailureCallback onConnectFailure;

	// Init
	public void Init() {
		// The config should look like this ip:port
		string[] databaseConfig = File.ReadAllText(databaseConfigPath).Split(':');
		
		// Since ip is the infront of : it will be index 0
		databaseIP = databaseConfig[0];
		
		// Since port is behind of : it will be index 1, we also have to convert it to an int
		databasePort = int.Parse(databaseConfig[1]);
		
		// Connect to DB
		Connect();
	}
	
	// Connect to the database
	void Connect() {
		Database.AddNode("Riak", databaseIP, databasePort, socketPoolSize, Defaults.WriteTimeout, Defaults.ReadTimeout);
		Database.Connect(onConnect, onConnectFailure);
	}

#region Properties
	// Database IP
	public string databaseIP {
		get;
		protected set;
	}
	
	// Database Port
	public int databasePort {
		get;
		protected set;
	}
#endregion
}