public interface IEmailService
{
    Task SendAlertAsync(string to, string from, string subject, string message, SmtpSettings settings, CancellationToken cancellationToken = default);
}
