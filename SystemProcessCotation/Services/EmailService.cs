using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailService : IEmailService
{
    public async Task SendAlertAsync(string to, string from, string subject, string message, SmtpSettings settings, CancellationToken cancellationToken = default)
    {
        try
        {
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(MailboxAddress.Parse(from));
            mailMessage.To.Add(MailboxAddress.Parse(to));
            mailMessage.Subject = subject;
            mailMessage.Body = new TextPart("plain") { Text = message };

            using var client = new SmtpClient();
            var secureOption = settings.EnableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None;

            await client.ConnectAsync(settings.Host, settings.Port, secureOption, cancellationToken);
            await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
            await client.SendAsync(mailMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error ao enviar email: {ex}");
        }
    }
}
