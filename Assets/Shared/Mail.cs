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
		smtpServer.Port = 587;
		smtpServer.Credentials = new System.Net.NetworkCredential("battleofmages@gmail.com", "bomlobbymail") as ICredentialsByHost;
		smtpServer.EnableSsl = true;
		ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			return true;
		};
		
		try {
			smtpServer.Send(mail);
		} catch(SmtpException e) {
			LogManager.General.LogError("Could not send mail message to: '" + to + "' (" + e.ToString() + ")");
		}
	}
}