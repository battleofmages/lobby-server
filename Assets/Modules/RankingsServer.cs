using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class RankingsServer : MonoBehaviour {
	private RankingsDB rankingsDB;
	
	void Start () {
		rankingsDB = this.GetComponent<RankingsDB>();
		
		// Init ranking lists
		GameDB.InitRankingLists();
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void RankingListRequest(byte subject, byte page, LobbyMessageInfo info) {
		uint maxPlayerCount = 10;
		
		//XDebug.Log("Retrieving top " + maxPlayerCount + " ranks");
		StartCoroutine(rankingsDB.GetTopRanks(subject, page, maxPlayerCount, info.sender));
	}
}
