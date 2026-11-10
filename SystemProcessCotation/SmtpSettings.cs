using MimeKit;
using System.Globalization;

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
        && IsValidUsername(Username)
        && IsValidPassword(Password);

    private static bool IsValidHost(string? value)
    {
        var host = value?.Trim();
        if (string.IsNullOrWhiteSpace(host)
            || host.Length > 253
            || host.Any(c => char.IsControl(c) || char.GetUnicodeCategory(c) == UnicodeCategory.Format))
        {
            return false;
        }

        var hostType = Uri.CheckHostName(host);
        return hostType != UriHostNameType.Unknown
            && (hostType != UriHostNameType.Dns
                || HasValidDnsLabels(host));
    }

    private static bool IsValidAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Any(c => char.IsControl(c) || char.GetUnicodeCategory(c) == UnicodeCategory.Format))
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
            && IsValidAddressDomain(parts[1]);
    }

    private static bool IsValidAddressDomain(string domain)
    {
        if (domain.Length > 253)
        {
            return false;
        }

        var hostType = Uri.CheckHostName(domain);
        return hostType != UriHostNameType.Unknown
            && (hostType != UriHostNameType.Dns || HasValidDnsLabels(domain));
    }

    private static bool HasValidDnsLabels(string host)
    {
        var dnsName = host.TrimEnd('.');
        return dnsName.Length > 0
            && dnsName.Split('.').All(IsValidDnsLabel);
    }

    private static bool IsValidDnsLabel(string label) =>
        label.Length is > 0 and <= 63
        && label[0] != '-'
        && label[^1] != '-'
        && label.All(c => char.IsAsciiLetterOrDigit(c) || c == '-');

    private static bool IsValidUsername(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && !value.Any(c => char.IsControl(c) || char.GetUnicodeCategory(c) == UnicodeCategory.Format);

    private static bool IsValidPassword(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && !value.Any(c => char.IsControl(c) || char.GetUnicodeCategory(c) == UnicodeCategory.Format);
}
