using uLobby;
using UnityEngine;

public class SkillBuildsServer : MonoBehaviour {
	// Start
	void Start () {
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
		
		SkillBuildsDB.SetSkillBuild(
			player.accountId,
			build,
			data => {
				if(data == null)
					Lobby.RPC("SkillBuildSaveError", info.sender);
			}
		);
	}
}
