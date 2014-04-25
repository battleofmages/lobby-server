using uLobby;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LobbyMatchMaker : SingletonMonoBehaviour<LobbyMatchMaker> {
	public LobbyQueue[] queue;
	public float queueStatsSendInterval;
	private int loggedPlayerCount;
	
	// Start
	void Start() {
		// Create queues
		queue = new LobbyQueue[5];
		for(int i = 0; i < queue.Length; i++) {
			queue[i] = new LobbyQueue();
			queue[i].unitsNeededForGameStart = (i + 1) * 2;
		}
		
		// Make this class listen to Lobby events
		Lobby.AddListener(this);
		
		// Send queue stats
		InvokeRepeating("SendQueueStats", 0.001f, queueStatsSendInterval);
	}
	
	// LeaveMatch
	void LeaveMatch(LobbyPlayer player) {
		if(player.inMatch)
			LogManager.General.Log("Player '" + player.name + "' left a match");
		else if(player.inFFA)
			LogManager.General.Log("Player '" + player.name + "' left an FFA server");
		else
			return;
		
		// A player just returned from a match, we'll send him queue infos again
		player.gameInstance = null;
		
		// Send him the chat members again to prevent wrong status info
		foreach(var channel in player.channels) {
			channel.SendMemberListToPlayer(player);
		}
		
		// Return him to the world
		player.ReturnToWorld();
	}
	
	// We send players information about the queues each second
	void SendQueueStats() {
		var offlinePlayers = new List<LobbyPlayer>();
		int playerCount = LobbyPlayer.list.Count;
		
		if(loggedPlayerCount != playerCount) {
			LogManager.Spam.Log("SendQueueStats [" + playerCount + " players] started.");
		}
		
		// TODO: Players need to request queue stats (to not send data to AFK players)
		foreach(var player in LobbyPlayer.list) {
			if(player.inMatch)
				continue;
			
			// If for some reason the player is still in the list after being disconnected
			// add him to the offlinePlayers list and remove him later.
			if(!Lobby.IsPeerConnected(player.peer)) {
				offlinePlayers.Add(player);
				continue;
			}
			
			// Send information to the player about the queues
			try {
				Lobby.RPC("QueueStats",
				          player.peer,
				          playerCount,
				          queue[0].playerCount,
				          queue[1].playerCount,
				          queue[2].playerCount,
				          queue[3].playerCount,
				          queue[4].playerCount
				          );
			} catch {
				LogManager.General.Log("Couldn't send queue data to player '" + player.name + "' from account '" + player.account.name + "'");
			}
		}
		
		// Clear offline players
		foreach(LobbyPlayer player in offlinePlayers) {
			LobbyServer.instance.OnPeerDisconnected(player.peer);
		}
		
		if(loggedPlayerCount != playerCount) {
			LogManager.Spam.Log("SendQueueStats [" + playerCount + " players] finished.");
			loggedPlayerCount = playerCount;
		}
	}
	
#region RPCs
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	[RPC]
	void JoinFFARequest(byte playersPerTeam, LobbyMessageInfo info) {
		var player = LobbyServer.GetLobbyPlayer(info);
		
		if(!player.inTown)
			return;
		
		// Start new town server if needed
		LobbyFFA ffaInstance = LobbyFFA.PickFFAInstance(player);
		
		// Connect the player once the instance is ready
		StartCoroutine(player.ConnectToGameInstanceDelayed(ffaInstance));
	}
	
	[RPC]
	void EnterQueue(byte playersPerTeam, LobbyMessageInfo info) {
		// Check for correct team size
		if(playersPerTeam == 0 || playersPerTeam > 5)
			return;
		
		var player = LobbyServer.GetLobbyPlayer(info);
		
		// Do we have ranking information?
		if(player.stats == null)
			return;
		
		if(!player.inTown)
			return;
		
		var enteredQueue = queue[playersPerTeam - 1];
		
		// Add the player to the queue
		if(enteredQueue.AddPlayer(player)) {
			// Let the player know he entered the queue
			LogManager.General.Log("Added '" + player.name + "' to " + playersPerTeam + "v" + playersPerTeam + " queue");
			Lobby.RPC("EnteredQueue", player.peer, playersPerTeam);
			
			// Make matches as new players joined
			if(LobbyInstanceManager.uZoneNodeCount > 0) {
				enteredQueue.MakeMatchesBasedOnRanking();
			}
		}
	}
	
	[RPC]
	void LeaveQueue(LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		// Make the player leave the queue
		if(player.LeaveQueue()) {
			// Let the player know he left the queue
			LogManager.General.Log("'" + player.name + "' left the queue");
			Lobby.RPC("LeftQueue", player.peer);
		}
	}
	
	[RPC]
	void AcceptMatch(bool accept, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		if(player.instanceAwaitingAccept == null)
			return;
		
		if(accept) {
			player.AcceptMatch();
		} else {
			player.DenyMatch();
		}
	}
	
	[RPC]
	IEnumerator LeaveInstance(bool gameEnded, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		if(player.inMatch) {
			LogManager.General.Log("Player '" + player.name + "' returned from a match");
			
			if(gameEnded) {
				// Send him his new stats
				LobbyGameDB.GetPlayerStats(player);
				
				// Send him his new artifact inventory
				ArtifactsDB.GetArtifactInventory(player);
				
				// Update ranking list cache
				if(!player.match.updatedRankingList) {
					RankingsServer.instance.StartRankingListCacheUpdate(player.match);
					player.match.updatedRankingList = true;
				}
			}
			
			LeaveMatch(player);
		} else if(player.inFFA) {
			// Send him his new stats
			//StartCoroutine(lobbyGameDB.GetPlayerStats(player));
			
			// Update ranking list cache
			/*if(!player.match.updatedRankingList) {
				RankingsServer.instance.StartRankingListCacheUpdate(player.match);
				player.match.updatedRankingList = true;
			}*/
			
			LeaveMatch(player);
		} else if(player.inTown) {
			player.gameInstance = null;
			
			if(AccountManager.Master.IsLoggedIn(player.peer)) {
				yield return AccountManager.Master.LogOut(info.sender).WaitUntilDone();
			}
		}
	}
#endregion
}
