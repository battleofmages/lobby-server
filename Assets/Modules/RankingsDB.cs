using UnityEngine;
using uLobby;
using uGameDB;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class RankingsDB : MonoBehaviour {
	// Get top ranks
	public IEnumerator GetTopRanks(uint maxPlayerCount, uLobby.LobbyPeer peer) {
		// TODO: Use GameDB.MapReduce
		
		// Retrieve the highscore list from the database by using MapReduce. The MapReduce request consists of a
		// map phase and a reduce phase. The phases are expressed as JavaScript code in string form. The reduce
		// phase also gets the maximum number of scores to fetch as an argument.
		var bucket = new Bucket("AccountToStats");
		var getHighscoresRequest = bucket.MapReduce(
			new JavaScriptMapPhase(highscoresMapFunction),
			new JavaScriptReducePhase(highscoresReduceFunction, maxPlayerCount)
		);
		
		// Wait until the request finishes and then update the local list of highscore entries.
		yield return getHighscoresRequest.WaitUntilDone();
		
		if(getHighscoresRequest.isSuccessful) {
			IEnumerable<RankEntry> rankingEntriesTmp = getHighscoresRequest.GetResult<RankEntry>();
			
			var rankingEntries = rankingEntriesTmp.ToArray();
			GameDB.rankingLists[0][0] = rankingEntries;
			
			// Get player names
			// TODO: Send X requests at once, then wait for all of them
			var nameBucket = new Bucket("AccountToName");
			var nameRequests = new GetRequest[rankingEntries.Length];
			for(int i = 0; i < rankingEntries.Length; i++) {
				var entry = rankingEntries[i];
				entry.rankIndex = i;
				
				if(GameDB.accountIdToName.ContainsKey(entry.accountId)) {
					entry.name = GameDB.accountIdToName[entry.accountId];
					nameRequests[i] = null;
				} else {
					nameRequests[i] = nameBucket.Get(entry.accountId);
				}
			}
			
			for(int i = 0; i < nameRequests.Length; i++) {
				var nameRequest = nameRequests[i];
				if(nameRequest == null)
					continue;
				
				yield return nameRequest.WaitUntilDone();
				
				if(nameRequest.isSuccessful) {
					var entry = rankingEntries[i];
					entry.name = nameRequest.GetValue<string>();
					GameDB.accountIdToName[entry.accountId] = entry.name;
				}
			}
			
			//XDebug.Log("Sending the ranking list " + GameDB.rankingEntries + " with " + rankingEntries.Length + " entries");
			Lobby.RPC("ReceiveRankingList", peer, rankingEntries, false);
		} else {
			XDebug.Log("Failed getting the ranking list: " + getHighscoresRequest.GetErrorString());
			Lobby.RPC("ReceiveRankingList", peer, null, false);
		}
	}
	
	// --------------------------------------------------------------------------------
	// MapReduce: Rankings
	// --------------------------------------------------------------------------------
	
	// This is the JavaScript code for the map phase. The map phase operates on each key/value pair in the bucket
	// and should produce a list of any length. The list is then concatenated with the output of other map
	// operations and fed into the reduce phase. This map phase just parses the text value to a JSON object and
	// returns it as a one-element list.
	private const string highscoresMapFunction =
	@"
	function(value, keydata, arg) {
		var scoreEntry = JSON.parse(value.values[0].data);
		return [[0, value.key, '', scoreEntry.bestRanking, scoreEntry.total.damage]];
	}
	";
	
	// This is the JavaScript code for the reduce phase. The reduce phase operates on a combined list of the results
	// from any number of map phases, and should produce a new list. The resulting list can then be combined with
	// even more map phase results and fed into another reduce phase, so it is important that the reduce function
	// can be run many times on the same data without failing. This reduce phase sorts the items in the list
	// by score and trims away any item beyond the maxScoreCount argument that was sent along with the request.
	private const string highscoresReduceFunction =
	@"
	function(valueList, maxScoreCount) {
		var descendingOrder = function(a, b) {
			var diff = b[3] - a[3];
			
			if(diff == 0)
				return b[4] - a[4];
			
			return diff;
		};
		valueList.sort(descendingOrder);
		if (valueList.length > maxScoreCount) { valueList.length = maxScoreCount; }
		return valueList;
	}
	";
}
