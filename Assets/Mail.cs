using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class Mail {
	public static void Send(string to, string subject, string body, string mailFrom = "noreply@battle-of-mages.com") {
		MailMessage mail = new MailMessage();
		
		mail.From = new MailAddress(mailFrom);
		mail.To.Add(to);
		mail.Subject = subject;
		mail.Body = body;
		
		SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
		smtpServer.Port = 587; // 465
		smtpServer.Credentials = new System.Net.NetworkCredential("battleofmages@gmail.com", "bomlobbymail") as ICredentialsByHost;
		smtpServer.EnableSsl = true;
		smtpServer.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
		ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			return true;
		};
		
		try {
			smtpServer.SendAsync(mail, to);
		} catch(SmtpException e) {
			LogManager.General.LogError("Could not send mail message to: '" + to + "' (" + e.ToString() + ")");
		}
	}
	
	// On completion
	private static void SendCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
		// Get the unique identifier for this asynchronous operation.
		string token = (string)(e.UserState);
		
		if(e.Cancelled) {
			LogManager.General.LogError(string.Format("[{0}] E-Mail sending canceled", token));
		}
		
		if(e.Error != null) {
			LogManager.General.LogError(string.Format("[{0}] E-Mail send error: {1}", token, e.Error.ToString()));
		} else {
			LogManager.General.Log(string.Format("Sent activation mail to '{0}'", token));
		}
	}
}