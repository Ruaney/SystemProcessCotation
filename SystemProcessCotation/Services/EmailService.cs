using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailService : IEmailService
{
    public async Task SendAlertAsync(string to, string from, string subject, string message, SmtpSettings settings, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var fromAddress = ParseRequiredMailboxAddress(from, nameof(from));
            var toAddress = ParseRequiredMailboxAddress(to, nameof(to));
            var host = GetRequiredHost(settings.Host);
            var username = GetRequiredText(settings.Username, nameof(settings.Username));
            var password = GetRequiredText(settings.Password, nameof(settings.Password));
            var port = GetRequiredPort(settings.Port);

            var mailMessage = new MimeMessage();
            mailMessage.From.Add(fromAddress);
            mailMessage.To.Add(toAddress);
            mailMessage.Subject = subject;
            mailMessage.Body = new TextPart("plain") { Text = message };

            using var client = new SmtpClient();
            var secureOption = settings.EnableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None;

            await client.ConnectAsync(host, port, secureOption, cancellationToken);
            await client.AuthenticateAsync(username, password, cancellationToken);
            await client.SendAsync(mailMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao enviar email.", ex);
        }
    }

    private static MailboxAddress ParseRequiredMailboxAddress(string address, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new FormatException($"O endereço de email '{parameterName}' é obrigatório.");
        }

        return MailboxAddress.Parse(address.Trim());
    }

    private static string GetRequiredHost(string host)
    {
        var normalized = GetRequiredText(host, nameof(SmtpSettings.Host));
        if (Uri.CheckHostName(normalized) == UriHostNameType.Unknown)
        {
            throw new FormatException("O host SMTP deve ser um nome DNS ou endereço IP válido.");
        }

        return normalized;
    }

    private static string GetRequiredText(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new FormatException($"O campo SMTP '{fieldName}' é obrigatório.");
        }

        return value.Trim();
    }

    private static int GetRequiredPort(int port)
    {
        if (port is <= 0 or > 65535)
        {
            throw new FormatException("A porta SMTP deve estar entre 1 e 65535.");
        }

        return port;
    }
}
