using UnityEngine;
using System.Collections;

[System.Serializable]
public class CharacterStats {
	public int attack;
	public int defense;
	public int block;
	public int cooldownReduction;
	public int attackSpeed;
	public int moveSpeed;
	
	// Empty constructor
	public CharacterStats() {
		attack = 50;
		defense = 50;
		block = 50;
		cooldownReduction = 50;
		attackSpeed = 50;
		moveSpeed = 50;
	}
	
	// Default value - needs to be separate from empty constructor
	public CharacterStats(int defaultValue) {
		attack = defaultValue;
		defense = defaultValue;
		block = defaultValue;
		cooldownReduction = defaultValue;
		attackSpeed = defaultValue;
		moveSpeed = defaultValue;
	}
	
	// Copy constructor
	public CharacterStats(CharacterStats other) {
		attack = other.attack;
		defense = other.defense;
		block = other.block;
		cooldownReduction = other.cooldownReduction;
		attackSpeed = other.attackSpeed;
		moveSpeed = other.moveSpeed;
	}
	
	public bool Compare(CharacterStats other) {
		return
			attack == other.attack &&
			defense == other.defense && 
			block == other.block && 
			cooldownReduction == other.cooldownReduction && 
			attackSpeed == other.attackSpeed && 
			moveSpeed == other.moveSpeed;
	}
	
	public void ApplyOffset(int val) {
		attack += val;
		defense += val;
		block += val;
		cooldownReduction += val;
		attackSpeed += val;
		moveSpeed += val;
	}
	
	public void ApplyOffset(CharacterStats stats) {
		attack += stats.attack;
		defense += stats.defense;
		block += stats.block;
		cooldownReduction += stats.cooldownReduction;
		attackSpeed += stats.attackSpeed;
		moveSpeed += stats.moveSpeed;
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<CharacterStats>(writer, (CharacterStats)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<CharacterStats>(reader);
	}
	
	public string multiLineString {
		get {
			var s = "\n";
			return string.Concat(
				"Attack +", attack, s,
				"Defense +", defense, s,
				"Energy +", block, s,
				"CD reduction +", cooldownReduction, s,
				"Attack speed +", attackSpeed, s,
				"Move speed +", moveSpeed
			);
		}
	}
	
	public override string ToString() {
		var s = " / ";
		return attack + s + defense + s + block + s + cooldownReduction + s + attackSpeed + s + moveSpeed;
	}
	
	public int totalStatPointsUsed {
		get {
			return
				attack +
				defense +
				block +
				cooldownReduction + 
				attackSpeed +
				moveSpeed;
		}
	}
	
	public int maxStatPoints {
		get { return 300; }
	}
	
	public int statPointsLeft {
		get { return maxStatPoints - totalStatPointsUsed; }
	}
	
	public float attackDmgMultiplier {
		get { return 1.0f + ((attack - 50) * 0.005f); }
	}
	
	public float defenseDmgMultiplier {
		get { return 1.0f - ((defense - 50) * 0.003333f); }
	}
	
	public float blockMaxCapacityMultiplier {
		get {
			return 1.0f + ((block - 50) * 0.0075f);
		}
	}
	
	public float cooldownMultiplier {
		get { return 1.0f - ((cooldownReduction - 50) * 0.005f); }
	}
	
	public float attackSpeedMultiplier {
		get { return 1.0f - ((attackSpeed - 50) * 0.005f); }
	}
	
	public float moveSpeedMultiplier {
		get { return 1.0f + ((moveSpeed - 50) * 0.005f); }
	}
}
