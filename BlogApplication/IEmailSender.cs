namespace BlogApplication
{
    public interface IEmailSender
    {
        Task SendEmailConfirmationAsync(string email, string message,string subject);
    }
}
