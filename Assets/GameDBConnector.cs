using UnityEngine;
using System.IO;
using uGameDB;

public class GameDBConnector : SingletonMonoBehaviour<GameDBConnector> {
	[HideInInspector]
	public string databaseIP;

	[HideInInspector]
	public int databasePort;

	public string databaseConfigPath;

	private int socketPoolSize = 10;

	// Start
	void Start() {
		// The config should look like this ip:port
		string[] databaseConfig = File.ReadAllText(databaseConfigPath).Split(':');

		// Since ip is the infront of : it will be 0
		databaseIP = databaseConfig[0];

		// Since port is behind of : it will be 1, we also have to convert it to a int
		databasePort = int.Parse(databaseConfig[1]);

		// Connect to DB
		Connect();
	}

	// Connect to the database
	void Connect() {
		Database.AddNode("Riak", databaseIP, databasePort, socketPoolSize, Defaults.WriteTimeout, Defaults.ReadTimeout);
		Database.Connect();
	}
}