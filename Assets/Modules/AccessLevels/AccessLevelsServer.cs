using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class AccessLevelsServer : MonoBehaviour {
	//private AccessLevelsDB accessLevelsDB;
	
	void Start () {
		//accessLevelsDB = this.GetComponent<AccessLevelsDB>();
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
}
