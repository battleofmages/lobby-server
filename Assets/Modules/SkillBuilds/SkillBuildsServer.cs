using uLobby;
using UnityEngine;
using System.Collections;

public class SkillBuildsServer : MonoBehaviour {
	private SkillBuildsDB skillBuildsDB;
	
	void Start () {
		skillBuildsDB = this.GetComponent<SkillBuildsDB>();
		
		// Make this class listen to lobby events
		Lobby.AddListener(this);
	}
	
	// --------------------------------------------------------------------------------
	// RPCs
	// --------------------------------------------------------------------------------
	
	[RPC]
	void ClientSkillBuild(SkillBuild build, LobbyMessageInfo info) {
		LobbyPlayer player = LobbyServer.GetLobbyPlayer(info);
		
		// TODO: Check the build for hacks
		
		StartCoroutine(skillBuildsDB.SetSkillBuild(
			player.accountId,
			build,
			data => {
				if(data == null)
					Lobby.RPC("SkillBuildSaveError", info.sender);
			}
		));
	}
}
