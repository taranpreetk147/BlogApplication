using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;

namespace BlogApplication
{
    public class EmailSender : IEmailSender
    {
        private EmailSettings _emailSettings { get; }
        public EmailSender(IOptions<EmailSettings> emailSettings)
        {            
            _emailSettings = emailSettings.Value;
        }
        public async Task Execute(string email, string subject, string message)
        {
            try
            {
                string toEmail = string.IsNullOrEmpty(email) ? _emailSettings.ToEmail : email;
                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.UserNameEmail, "My Email Display Name")
                };
                mailMessage.To.Add(toEmail);
                mailMessage.To.Add(_emailSettings.CcEmail);
                mailMessage.Subject = "Blog Application:" + subject;
                mailMessage.Body = message;
                mailMessage.IsBodyHtml = true;
                mailMessage.Priority = MailPriority.High;
                using (SmtpClient smtp = new SmtpClient(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort))
                {
                    smtp.Credentials = new NetworkCredential(_emailSettings.UserNameEmail, _emailSettings.UserNamePassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mailMessage);
                };
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        public async Task SendEmailConfirmationAsync(string email, string message,string subject)
        {
            await Execute(email, subject,message);
        }

    }
}
