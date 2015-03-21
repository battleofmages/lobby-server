using UnityEngine;
using System.Collections;

public class Mailer : SingletonMonoBehaviour<Mailer> {
	public string resendActivationMailURL;

	// Sends account activation mail
	public Coroutine SendActivationMail(string email) {
		return StartCoroutine(SendActivationMailCoroutine(email));
	}
	
	// SendActivationMailCoroutine
	IEnumerator SendActivationMailCoroutine(string email) {
		LogManager.General.Log("Sending activation mail to '" + email + "'...");
		
		var sendMailRequest = new WWW(resendActivationMailURL.Replace("{email}", email));
		yield return sendMailRequest;
		
		if(sendMailRequest.error == null) {
			LogManager.General.Log("Sent activation mail to '" + email + "'");
		} else {
			LogManager.General.LogError("Failed sending activation mail to '" + email + "'");
		}
	}
}
