using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class DonationsServer : MonoBehaviour {
	private DonationsDB donationsDB;
	
	void Start () {
		donationsDB = this.GetComponent<DonationsDB>();
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	IEnumerator CrystalBalanceRequest(LobbyMessageInfo info) {
		LobbyPlayer lobbyPlayer = LobbyServer.GetLobbyPlayer(info);
		
		yield return StartCoroutine(donationsDB.GetPaymentsList(lobbyPlayer.account.id.value, data => {
			if(data == null) {
				Lobby.RPC("ReceiveCrystalBalance", lobbyPlayer.peer, 0);
			} else {
				Lobby.RPC("ReceiveCrystalBalance", lobbyPlayer.peer, (int)(data.balance * 100));
			}
		}));
	}
}
