using UnityEngine;
using System.Collections;

public enum OnlineStatus {
	Offline,
	Online,
	InQueue,
	InMatch,
	AFK
}

public class ChatMember {
	public string name;
	public OnlineStatus status;

	// Constructor
	public ChatMember() {
		name = "";
		status = OnlineStatus.Offline;
	}

	// Constructor
	public ChatMember(string nName) {
		name = nName;
		status = OnlineStatus.Offline;
	}

	// Constructor
	public ChatMember(string nName, OnlineStatus nStatus) {
		name = nName;
		status = nStatus;
	}

	// Constructor
	public ChatMember(string nName, byte nStatus) {
		name = nName;
		status = (OnlineStatus)nStatus;
	}
	
	public static void WriteToBitStream(uLink.BitStream stream, object val, params object[] args) {
		ChatMember myObj = (ChatMember)val;
		stream.WriteString(myObj.name);
		stream.WriteByte((byte)myObj.status);
	}
	
	public static object ReadFromBitStream(uLink.BitStream stream, params object[] args) {
		ChatMember myObj = new ChatMember(stream.ReadString(), stream.ReadByte());
		return myObj;
	}
}
