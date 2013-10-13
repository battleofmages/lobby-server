using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using uLobby;

public class SkillBuildsDB : MonoBehaviour {
	// --------------------------------------------------------------------------------
	// AccountToSkillBuild
	// --------------------------------------------------------------------------------
	
	// Get skill build
	public IEnumerator GetSkillBuild(string accountId, GameDB.ActionOnResult<SkillBuild> func) {
		yield return StartCoroutine(GameDB.Get<SkillBuild>(
			"AccountToSkillBuild",
			accountId,
			func
		));
	}
	
	// Set skill build
	public IEnumerator SetSkillBuild(string accountId, SkillBuild skillBuild, GameDB.ActionOnResult<SkillBuild> func = null) {
		yield return StartCoroutine(GameDB.Set<SkillBuild>(
			"AccountToSkillBuild",
			accountId,
			skillBuild,
			func
		));
	}
}
