using UnityEngine;
using System.Collections;

public enum ChatMemberStatus {
	Offline,
	Online,
	InQueue,
	InMatch,
	AFK
}

public class ChatMember {
	public string name;
	public ChatMemberStatus status;

	// Constructor
	public ChatMember() {
		name = "";
		status = ChatMemberStatus.Offline;
	}

	// Constructor
	public ChatMember(string nName) {
		name = nName;
		status = ChatMemberStatus.Offline;
	}

	// Constructor
	public ChatMember(string nName, ChatMemberStatus nStatus) {
		name = nName;
		status = nStatus;
	}

	// Constructor
	public ChatMember(string nName, byte nStatus) {
		name = nName;
		status = (ChatMemberStatus)nStatus;
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
