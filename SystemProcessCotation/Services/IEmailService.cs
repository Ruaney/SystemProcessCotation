public interface IEmailService
{
    Task sendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}