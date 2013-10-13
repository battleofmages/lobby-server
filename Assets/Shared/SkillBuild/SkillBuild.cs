using UnityEngine;
using System.Collections;

[System.Serializable]
public class SkillBuild {
	public WeaponBuild[] weapons;
	
	public SkillBuild() {
		
	}
	
	public bool HasSkill(int findSkillId) {
		foreach(var weaponBuild in weapons) {
			foreach(var attunementBuild in weaponBuild.attunements) {
				foreach(int skillId in attunementBuild.skills) {
					if(skillId == findSkillId)
						return true;
				}
			}
		}
		
		return false;
	}
	
	public WeaponBuild GetWeaponBuildById(int id) {
		foreach(var weaponBuild in weapons) {
			if(weaponBuild.weaponId == id) {
				return weaponBuild;
			}
		}
		
		return null;
	}
	
	public static SkillBuild GetStarterBuild() {
		var build = new SkillBuild();
		
		build.weapons = new WeaponBuild[] {
			// No weapon
			new WeaponBuild {
				weaponId = 0,
				attunements = new AttunementBuild[] {
					// Fire
					new AttunementBuild {
						attunementId = 10,
						skills = new int[] {
							100,
							101,
							102,
							103,
							104
						}
					},
					
					// Ice
					new AttunementBuild {
						attunementId = 11,
						skills = new int[] {
							200,
							201,
							202,
							203,
							204
						}
					},
					
					// Dark
					new AttunementBuild {
						attunementId = 20,
						skills = new int[] {
							300,
							301,
							302,
							303,
							304
						}
					},
					
					// Light
					new AttunementBuild {
						attunementId = 21,
						skills = new int[] {
							400,
							401,
							402,
							403,
							404
						}
					},
				}
			}
		};
		
		return build;
	}
	
	// BitStream Writer
	public static void WriteToBitStream(uLink.BitStream stream, object val, params object[] args) {
		var obj = (SkillBuild)val;
		
		stream.Write<WeaponBuild[]>(obj.weapons);
	}
	
	// BitStream Reader
	public static object ReadFromBitStream(uLink.BitStream stream, params object[] args) {
		var obj = new SkillBuild();
		
		obj.weapons = stream.Read<WeaponBuild[]>();
		
		return obj;
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<SkillBuild>(writer, (SkillBuild)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<SkillBuild>(reader);
	}
}
