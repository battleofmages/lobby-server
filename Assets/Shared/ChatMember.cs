using UnityEngine;
using System.Collections;

public class ChatMember {
	// TODO: Change to account IDs
	public string accountId;

	// Constructor
	public ChatMember() {
		accountId = "";
	}

	// Constructor
	public ChatMember(string nName) {
		accountId = nName;
	}
	
	public static void WriteToBitStream(uLink.BitStream stream, object val, params object[] args) {
		ChatMember myObj = (ChatMember)val;
		stream.WriteString(myObj.accountId);
	}
	
	public static object ReadFromBitStream(uLink.BitStream stream, params object[] args) {
		ChatMember myObj = new ChatMember(stream.ReadString());
		return myObj;
	}
}
