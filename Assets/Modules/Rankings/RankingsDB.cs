using UnityEngine;
using uLobby;
using uGameDB;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class RankingsDB : MonoBehaviour {
	public static string[] pageToPropertyName = {
		"bestRanking",
		"total.ranking",
		"queue[0].ranking",
		"queue[1].ranking",
		"queue[2].ranking",
		"queue[3].ranking",
		"queue[4].ranking",
	};
	
	// Get top ranks
	public IEnumerator GetTopRanks(byte subject, byte page, uint maxPlayerCount, GameDB.ActionOnResult<RankEntry[]> func = null) {
		// TODO: Use GameDB.MapReduce
		
		// Retrieve the highscore list from the database by using MapReduce. The MapReduce request consists of a
		// map phase and a reduce phase. The phases are expressed as JavaScript code in string form. The reduce
		// phase also gets the maximum number of scores to fetch as an argument.
		var bucket = new Bucket("AccountToStats");
		var getHighscoresRequest = bucket.MapReduce(
			new JavaScriptMapPhase(GetHighscoresMapFunction(page)),
			new JavaScriptReducePhase(highscoresReduceFunction, maxPlayerCount)
		);
		
		// Wait until the request finishes and then update the local list of highscore entries.
		yield return getHighscoresRequest.WaitUntilDone();
		
		if(getHighscoresRequest.isSuccessful) {
			IEnumerable<RankEntry> rankingEntriesTmp = getHighscoresRequest.GetResult<RankEntry>();
			
			var rankingEntries = rankingEntriesTmp.ToArray();
			
			// Get player names
			// TODO: Send X requests at once, then wait for all of them
			var nameBucket = new Bucket("AccountToName");
			var countryBucket = new Bucket("AccountToCountry");
			
			var nameRequests = new GetRequest[rankingEntries.Length];
			var countryRequests = new GetRequest[rankingEntries.Length];
			
			for(int i = 0; i < rankingEntries.Length; i++) {
				var entry = rankingEntries[i];
				entry.rankIndex = i;
				
				// Name
				string name;
				if(GameDB.accountIdToName.TryGetValue(entry.accountId, out name)) {
					entry.name = name;
					nameRequests[i] = null;
				} else {
					nameRequests[i] = nameBucket.Get(entry.accountId);
				}
				
				// Country
				string country;
				if(IPInfoServer.accountIdToCountry.TryGetValue(entry.accountId, out country)) {
					entry.country = country;
					countryRequests[i] = null;
				} else {
					countryRequests[i] = countryBucket.Get(entry.accountId);
				}
			}
			
			for(int i = 0; i < rankingEntries.Length; i++) {
				// Name
				var nameRequest = nameRequests[i];
				if(nameRequest != null) {
					yield return nameRequest.WaitUntilDone();
					
					if(nameRequest.isSuccessful) {
						var entry = rankingEntries[i];
						entry.name = nameRequest.GetValue<string>();
						GameDB.accountIdToName[entry.accountId] = entry.name;
					}
				}
				
				// Country
				var countryRequest = countryRequests[i];
				if(countryRequest != null) {
					yield return countryRequest.WaitUntilDone();
					
					var entry = rankingEntries[i];
					
					if(countryRequest.isSuccessful) {
						entry.country = countryRequest.GetValue<string>();
						IPInfoServer.accountIdToCountry[entry.accountId] = entry.country;
					} else {
						entry.country = "";
						IPInfoServer.accountIdToCountry[entry.accountId] = "";
					}
				}
			}
			
			// Save in cache
			GameDB.rankingLists[subject][page] = rankingEntries;
			
			//LogManager.General.Log("Sending the ranking list " + rankingEntries + " with " + rankingEntries.Length + " / " + maxPlayerCount + " entries (" + subject + ", " + page + ")");
			if(func != null)
				func(rankingEntries);
		} else {
			LogManager.General.LogError("Failed getting the ranking list: " + getHighscoresRequest.GetErrorString());
			
			if(func != null)
				func(null);
		}
	}
	
	// --------------------------------------------------------------------------------
	// MapReduce: Rankings
	// --------------------------------------------------------------------------------
	
	// This is the JavaScript code for the map phase. The map phase operates on each key/value pair in the bucket
	// and should produce a list of any length. The list is then concatenated with the output of other map
	// operations and fed into the reduce phase. This map phase just parses the text value to a JSON object and
	// returns it as a one-element list.
	public static string GetHighscoresMapFunction(byte page) {
		return @"
			function(value, keydata, arg) {
				var scoreEntry = JSON.parse(value.values[0].data);
				return [[
					0,
					value.key,
					'',		// Name
					'',		// Country
					scoreEntry." + RankingsDB.pageToPropertyName[page] + @",
					scoreEntry.total.damage
				]];
			}
		";
	}
	
	// This is the JavaScript code for the reduce phase. The reduce phase operates on a combined list of the results
	// from any number of map phases, and should produce a new list. The resulting list can then be combined with
	// even more map phase results and fed into another reduce phase, so it is important that the reduce function
	// can be run many times on the same data without failing. This reduce phase sorts the items in the list
	// by score and trims away any item beyond the maxScoreCount argument that was sent along with the request.
	private const string highscoresReduceFunction =
	@"
	function(valueList, maxScoreCount) {
		var descendingOrder = function(a, b) {
			var diff = b[4] - a[4];
			
			if(diff == 0)
				return b[5] - a[5];
			
			return diff;
		};
		
		// Sort
		valueList.sort(descendingOrder);
		
		// Shorten
		if(valueList.length > maxScoreCount) {
			valueList.length = maxScoreCount;
		}
		
		// Remove entries with 0 ranking
		for(var i = valueList.length - 1; i >= 0; i--) {
			if(valueList[i][4] === 0) {
				valueList.splice(i, 1);
			} else {
				break;
			}
		}
		
		return valueList;
	}
	";
}
