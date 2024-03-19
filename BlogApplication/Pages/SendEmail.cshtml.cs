using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace BlogApplication.Pages
{
    public class SendEmailModel : PageModel
    {       
        public string Email { get; set; }
            private EmailSettings _emailSettings { get; }
            public SendEmailModel(IOptions<EmailSettings> emailSettings)
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
                        From = new MailAddress(_emailSettings.FromEmail, "My Email Display Name")
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
            public Task SendEmailAsync(string email, string subject, string htmlMessage)
            {
                Execute(email, subject, htmlMessage).Wait();
                return Task.FromResult(0);
            }

        }
    }
