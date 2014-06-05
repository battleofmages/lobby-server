using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class Mail {
	public static string smtpServer = "smtp.gmail.com";
	public static string smtpUser;
	public static string smtpPassword;
	
	public static void Send(string to, string subject, string body, string mailFrom = "noreply@battleofmages.com") {
		MailMessage mail = new MailMessage();
		
		mail.From = new MailAddress(mailFrom);
		mail.To.Add(to);
		mail.Subject = subject;
		mail.Body = body;
		
		SmtpClient smtpClient = new SmtpClient(smtpServer);
		smtpClient.Port = 587; // 465
		smtpClient.Credentials = new NetworkCredential(smtpUser, smtpPassword) as ICredentialsByHost;
		smtpClient.EnableSsl = true;
		smtpClient.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
		ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			return true;
		};
		
		try {
			smtpClient.SendAsync(mail, to);
		} catch(SmtpException e) {
			LogManager.General.LogError("Could not send mail message to: '" + to + "' (" + e + ")");
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
			LogManager.General.LogError(string.Format("[{0}] E-Mail send error: {1}", token, e.Error));
		} else {
			LogManager.General.Log(string.Format("Sent activation mail to '{0}'", token));
		}
	}
}