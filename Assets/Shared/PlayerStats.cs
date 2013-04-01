using System;
using System.Reflection;
using UnityEngine;

public class QueueSettings {
	public static int queueCount = 5;
	public static int queueIndex = -1;
}

public class GenericSerializer {
	// Write a single value in JSON
	public static void WriteJSONValue(Jboy.JsonWriter writer, object val) {
		if(val is long) {
			writer.WriteNumber((double)((long)val));
		} else if(val is double) {
			writer.WriteNumber((double)val);
		} else if(val is PlayerQueueStats) {
			GenericSerializer.WriteJSONClassInstance<PlayerQueueStats>(writer, (PlayerQueueStats)val);
		} else if(val is PlayerQueueStats[]) {
			writer.WriteObjectStart();
			
			PlayerQueueStats[] valArray = (PlayerQueueStats[])val;
			for(int i = 0; i < QueueSettings.queueCount; i++) {
				writer.WritePropertyName(i.ToString());
				GenericSerializer.WriteJSONClassInstance<PlayerQueueStats>(writer, valArray[i]);
			}
			
			writer.WriteObjectEnd();
		} else {
			writer.WriteNumber((double)((int)val));
		}
	}
	
	// Writes all fields of a class instance
	public static void WriteJSONClassInstance<T>(Jboy.JsonWriter writer, T instance) {
		// Type pointer
		Type type = typeof(T);
		
		// Obtain all fields
		FieldInfo[] fields = type.GetFields();
		
		// Loop through all fields
		writer.WriteObjectStart();
		
		foreach(var field in fields) {
			// Get property name and value
			string name = field.Name;
			object val = field.GetValue(instance);
			
			//Debug.Log("Writing '" + name + "'"); // with value '" + ((double)val).ToString() + "'");
			//Debug.Log("ValueType " + val.GetType().ToString());
			//Debug.Log("Value " + val.ToString());
			
			// Write them to the JSON stream
			writer.WritePropertyName(name);
			GenericSerializer.WriteJSONValue(writer, val);
		}
		
		writer.WriteObjectEnd();
	}
	
	// Read a single value from JSON
	public static object ReadJSONValue(Jboy.JsonReader reader, FieldInfo field) {
		if(field.FieldType == typeof(long)) {
			return (long)(reader.ReadNumber());
		} else if(field.FieldType == typeof(double)) {
			return reader.ReadNumber();
		} else if(field.FieldType == typeof(PlayerQueueStats)) {
			return GenericSerializer.ReadJSONClassInstance<PlayerQueueStats>(reader);
		} else if(field.FieldType == typeof(PlayerQueueStats[])) {
			reader.ReadObjectStart();
			
			PlayerQueueStats[] valArray = new PlayerQueueStats[QueueSettings.queueCount];
			for(int i = 0; i < QueueSettings.queueCount; i++) {
				reader.ReadPropertyName(i.ToString());
				valArray[i] = GenericSerializer.ReadJSONClassInstance<PlayerQueueStats>(reader);
			}
			
			reader.ReadObjectEnd();
			
			return valArray;
		} else {
			return (int)(reader.ReadNumber());
		}
	}
	
	// Read a new class instance from JSON
	public static T ReadJSONClassInstance<T>(Jboy.JsonReader reader) where T : new() {
		T instance = new T();
		
		reader.ReadObjectStart();
		
		// Obtain all fields
		FieldInfo[] fields = typeof(T).GetFields();
		
		// Loop through all fields
		foreach (var field in fields) {
			// Read and assign the value to the object
			reader.ReadPropertyName(field.Name);
			field.SetValue(instance, GenericSerializer.ReadJSONValue(reader, field));
		}
		
		reader.ReadObjectEnd();
		
		return instance;
	}
}

[Serializable]
public class PlayerQueueStats {
	public int ranking;
	public int rankingOffset;
	
	public int kills;
	public int deaths;
	public int assists;
	
	public int wins;
	public int losses;
	public int leaves;
	
	public int hits;
	public int hitsTaken;
	
	public int blocks;
	public int blocksTaken;
	
	public int lifeDrain;
	public int lifeDrainTaken;
	
	public long damage;
	public long damageTaken;
	
	public long cc;
	public long ccTaken;
	
	public int runeDetonations;
	public int runeDetonationsTaken;
	
	public int runeDetonationsLevel;
	public int runeDetonationsLevelTaken;
	
	public int topScorerOwnTeam;
	public int topScorerAllTeams;
	
	public double secondsPlayed;
	
	// Ranking formula
	public void CalculateRanking() {
		int winLoseDifference = wins - losses;
		int killDeathDifference = kills - deaths;
		long damageDifference = damage - damageTaken;
		
		int actualRanking = (int)(
			winLoseDifference * 2
			+ killDeathDifference * 0.1f
			+ damageDifference * 0.0001f
		);
		
		// This is the ranking we'd currently have
		int currentRanking = actualRanking + rankingOffset;
		
		// If we are below 0, reset ranking offset
		if(currentRanking < 0) {
			rankingOffset = -actualRanking;
			ranking = 0;
		// Otherwise just accept the calculated rating with its offset
		} else {
			ranking = currentRanking;
		}
	}
	
	// Calculated stats
	public double dps {get{
		if(secondsPlayed == 0)
			return 0d;
		
		return damage / secondsPlayed;
	}}
	
	public double ccpm {get{
		if(secondsPlayed == 0)
			return 0d;
		
		return 60 * cc / secondsPlayed;
	}}
	
	public int matchesPlayed {get{
		return wins + losses + leaves;
	}}
	
	public float kdRatio {get{
			return (float)kills / deaths;
	}}
	
	public float kdaRatio {get{
		if(deaths == 0)
			return 0;
		
		return (float)(kills + assists) / deaths;
	}}
	
	public string kdaString {get{
		return averageKills.ToString("0.0") + " / " + averageDeaths.ToString("0.0") + " / " + averageAssists.ToString("0.0");
	}}
	
	public float averageKills {get{
		if(matchesPlayed == 0)
			return 0;
		
		return kills / matchesPlayed;
	}}
	
	public float averageDeaths {get{
		if(matchesPlayed == 0)
			return 0;
		
		return deaths / matchesPlayed;
	}}
	
	public float averageAssists {get{
		if(matchesPlayed == 0)
			return 0;
		
		return assists / matchesPlayed;
	}}
}

[Serializable]
public class PlayerStats {
	// Stats saved in the database
	public double level = 1;
	public int bestRanking;
	public int ping;
	
	// Total stats
	public PlayerQueueStats total;
	
	// Saved for each queue
	public PlayerQueueStats[] queue;
	
	// Constructor
	public PlayerStats() {
		total = new PlayerQueueStats();
		
		queue = new PlayerQueueStats[QueueSettings.queueCount];
		
		for(int i = 0; i < QueueSettings.queueCount; i++) {
			queue[i] = new PlayerQueueStats();
		}
	}
	
	// Calculates new stats
	public void MergeWithMatch(PlayerStats matchStats) {
		// Total stats
		MergeQueueStats(total, matchStats.total);
		
		// Current queue stats
		if(QueueSettings.queueIndex >= 0 && QueueSettings.queueIndex < queue.Length) {
			MergeQueueStats(queue[QueueSettings.queueIndex], matchStats.total);
		}
		
		level = CalculateLevel();
		bestRanking = ChooseBestRanking();
	}
	
	// Merges 2 queue stats
	void MergeQueueStats(PlayerQueueStats db, PlayerQueueStats matchStats) {
		db.kills += matchStats.kills;
		db.deaths += matchStats.deaths;
		db.assists += matchStats.assists;
		
		db.wins += matchStats.wins;
		db.losses += matchStats.losses;
		db.leaves += matchStats.leaves;
		
		db.hits += matchStats.hits;
		db.hitsTaken += matchStats.hitsTaken;
		
		db.blocks += matchStats.blocks;
		db.blocksTaken += matchStats.blocksTaken;
		
		db.lifeDrain += matchStats.lifeDrain;
		db.lifeDrainTaken += matchStats.lifeDrainTaken;
		
		db.damage += matchStats.damage;
		db.damageTaken += matchStats.damageTaken;
		
		db.cc += matchStats.cc;
		db.ccTaken += matchStats.ccTaken;
		
		db.runeDetonations += matchStats.runeDetonations;
		db.runeDetonationsTaken += matchStats.runeDetonationsTaken;
		
		db.runeDetonationsLevel += matchStats.runeDetonationsLevel;
		db.runeDetonationsLevelTaken += matchStats.runeDetonationsLevelTaken;
		
		db.topScorerOwnTeam += matchStats.topScorerOwnTeam;
		db.topScorerAllTeams += matchStats.topScorerAllTeams;
		
		db.secondsPlayed += matchStats.secondsPlayed;
		
		db.CalculateRanking();
	}
	
	double CalculateLevel() {
		double winValue = 0d;
		double loseValue = 0d;
		
		if(total.wins > 0)
			winValue = Math.Log(total.wins);
		
		if(total.losses > 0)
			loseValue = Math.Log(total.losses) / 2;
		
		return 1.0d + winValue + loseValue;
	}
	
	int ChooseBestRanking() {
		int tmpBestRanking = 0;
		
		for(int i = 0; i < QueueSettings.queueCount; i++) {
			int queueRanking = queue[i].ranking;
			
			if(queueRanking > tmpBestRanking) {
				tmpBestRanking = queueRanking;
			}
		}
		
		if(total.ranking > tmpBestRanking) {
			tmpBestRanking = total.ranking;
		}
		
		return tmpBestRanking;
	}
	
	// uLink Bitstream write
	/*public static void BitStreamSerializer(
		uLink.BitStream stream,
		object val,
		params object[] codecOptions
	) {
		PlayerStats stats = (PlayerStats)val;
		stream.Write<double>(stats.level);
		stream.Write<int>(stats.bestRanking);
		stream.Write<int>(stats.ping);
	}*/
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<PlayerStats>(writer, (PlayerStats)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<PlayerStats>(reader);
	}
	
	// Calculated stats
	public int ranking {get{
		return bestRanking;
	}}
}
