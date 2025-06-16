public interface IEmailService
{
    void SendAlert(string to, string from, string subject, string message, SmtpSettings settings);
}