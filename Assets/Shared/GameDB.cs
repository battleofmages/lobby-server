using UnityEngine;
using uGameDB;
using System.Collections;
using System.Collections.Generic;
using Jboy;
using uLobby;

public class GameDB {
	public static RankEntry[] rankingEntries;
	
	public static void InitCodecs() {
		// Register JSON codec for player statistics
		Json.AddCodec<PlayerStats>(PlayerStats.JsonDeserializer, PlayerStats.JsonSerializer);
		Json.AddCodec<CharacterStats>(CharacterStats.JsonDeserializer, CharacterStats.JsonSerializer);
		
		// Register JSON codec for rank entries
		uLink.BitStreamCodec.AddAndMakeArray<RankEntry>(RankEntry.ReadRankEntry, RankEntry.WriteRankEntry);
		Json.AddCodec<RankEntry>(RankEntry.JsonDeserializer, RankEntry.JsonSerializer);
	}
}
