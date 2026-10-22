using MimeKit;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool EnableSsl { get; set; } = true;
    public string FromAddress { get; set; } = string.Empty;
    public string ToAddress { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool IsConfigured =>
        IsValidHost(Host)
        && Port is > 0 and <= 65535
        && IsValidAddress(FromAddress)
        && IsValidAddress(ToAddress)
        && !string.IsNullOrWhiteSpace(Username)
        && !string.IsNullOrWhiteSpace(Password);

    private static bool IsValidHost(string value)
    {
        var host = value.Trim();
        if (string.IsNullOrWhiteSpace(host) || host.Length > 255 || host.Any(char.IsControl))
        {
            return false;
        }

        return Uri.CheckHostName(host) != UriHostNameType.Unknown;
    }

    private static bool IsValidAddress(string value)
    {
        if (value.Any(char.IsControl))
        {
            return false;
        }

        if (!MailboxAddress.TryParse(value, out var address))
        {
            return false;
        }

        var parts = address.Address.Split('@', 2, StringSplitOptions.TrimEntries);
        return parts.Length == 2
            && !string.IsNullOrWhiteSpace(parts[0])
            && !string.IsNullOrWhiteSpace(parts[1]);
    }
}
