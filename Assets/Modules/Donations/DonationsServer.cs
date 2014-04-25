using UnityEngine;
using System.Collections;
using uLobby;

public class DonationsServer : MonoBehaviour {
	// Start
	void Start() {
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	IEnumerator CrystalBalanceRequest(LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		yield return DonationsDB.GetPaymentsList(player.accountId, data => {
			if(data == null) {
				Lobby.RPC("ReceiveCrystalBalance", player.peer, player.accountId, 0);
			} else {
				Lobby.RPC("ReceiveCrystalBalance", player.peer, player.accountId, (int)(data.balance * 100));
			}
		});
	}
}
