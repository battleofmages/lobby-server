using UnityEngine;
using System.Collections;
using uLobby;

public class ItemInventoryServer : MonoBehaviour {
	//private ItemInventoryDB itemInventoryDB;
	
	void Start () {
		//itemInventoryDB = GetComponent<ItemInventoryDB>();
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	
}
