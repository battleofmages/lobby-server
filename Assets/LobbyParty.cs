using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LobbyParty : Party<LobbyPlayer> {
	public static Dictionary<int, LobbyParty> idToParty = new Dictionary<int, LobbyParty>();
	public static int autoIncrement = 0;
	
	// Constructor
	public LobbyParty() : base() {
		// Find a new ID
		_id = autoIncrement++;
		while(idToParty.ContainsKey(_id))
			_id = autoIncrement++;
		
		// Save in dictionary
		idToParty[_id] = this;
		
		Debug.Log("Created party with ID " + _id);
	}
	
	// Clear
	public void Clear() {
		this.RemoveAllMembers();
		idToParty.Remove(this.id);
	}
}
