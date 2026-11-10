using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Globalization;

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
        if (string.IsNullOrWhiteSpace(address) || address.Any(c => char.IsControl(c) || char.GetUnicodeCategory(c) == UnicodeCategory.Format))
        {
            throw new FormatException($"O endereço de email '{parameterName}' é obrigatório.");
        }

        return MailboxAddress.Parse(address.Trim());
    }

    private static string GetRequiredHost(string host)
    {
        var normalized = GetRequiredText(host, nameof(SmtpSettings.Host));
        var hostType = Uri.CheckHostName(normalized);
        var dnsName = normalized.TrimEnd('.');
        if (normalized.Any(c => char.IsControl(c) || char.GetUnicodeCategory(c) == UnicodeCategory.Format)
            || hostType == UriHostNameType.Unknown
            || hostType == UriHostNameType.Dns
                && (dnsName.Length == 0 || dnsName.Split('.').Any(label =>
                    label.Length is 0 or > 63
                    || label[0] == '-'
                    || label[^1] == '-')))
        {
            throw new FormatException("O host SMTP deve ser um nome DNS ou endereço IP válido.");
        }

        return normalized;
    }

    private static string GetRequiredText(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Any(c => char.IsControl(c) || char.GetUnicodeCategory(c) == UnicodeCategory.Format))
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
