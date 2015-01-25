using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class PartyServer : MonoBehaviour {
	// Start
	void Start () {
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}

#region RPCs
	[RPC]
	void InviteToParty(string accountId, LobbyMessageInfo info) {
		var player = LobbyPlayer.Get(info);
		var invitedAccount = PlayerAccount.Get(accountId);
		var invitedPlayer = LobbyPlayer.Get(invitedAccount);

		// Peer still connected?
		if(invitedPlayer == null || invitedPlayer.disconnected || invitedAccount.onlineStatus.value == OnlineStatus.Offline) {
			LogManager.General.LogWarning(string.Format("{0} failed sending a party invitation to {1}", player, invitedAccount));
			return;
		}
		
		LogManager.General.Log(string.Format("{0} sent a party invitation to {1}", player, invitedAccount));

		// Send invitation
		invitedPlayer.RPC("PartyInvitation", player.account.id);
	}

	[RPC]
	void AcceptPartyInvitation(string accountId, LobbyMessageInfo info) {
		var newMember = LobbyPlayer.Get(info);
		var inviter = LobbyPlayer.Get(accountId);

		inviter.account.party.Get(party => {
			if(!party.CanAdd(newMember.account)) {
				LogManager.General.LogWarning("Cannot add " + newMember + " to the party of " + inviter);
				//return;
			}

			party.Add(newMember.account);

			// Subscribe to online status
			foreach(var memberId in party.accountIds) {
				var member = LobbyPlayer.Get(memberId);

				if(member == null)
					continue;

				member.account.party.value = party;
			}

			//inviter.RPC("AcceptedPartyInvitation", newMember.account.id);
		});
	}

	[RPC]
	void DenyPartyInvitation(string accountId, LobbyMessageInfo info) {
		var player = LobbyPlayer.Get(info);
		var inviter = LobbyPlayer.Get(accountId);

		inviter.RPC("DeniedPartyInvitation", player.account.id);
	}
#endregion
}