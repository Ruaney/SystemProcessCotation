using System.Globalization;
using System.Text;

/// <summary>
/// Carrega as configurações de SMTP a partir das variáveis de ambiente (.env).
/// Os valores são opcionais: quando ausentes, o <see cref="SmtpSettings.IsConfigured"/>
/// fica falso e os alertas são apenas registrados em log.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    public SmtpSettings LoadSmtpSettings()
    {
        return new SmtpSettings
        {
            Host = GetEnv("HOST", "SMTP_HOST", "EMAIL_HOST", "MAIL_HOST"),
            Port = int.TryParse(GetEnv("PORT", "SMTP_PORT", "EMAIL_PORT", "MAIL_PORT"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var port) ? port : 0,
            FromAddress = GetEnv("FROM", "SMTP_FROM", "SMTP_FROM_ADDRESS", "EMAIL_FROM", "MAIL_FROM"),
            ToAddress = GetEnv("TO", "SMTP_TO", "SMTP_TO_ADDRESS", "EMAIL_TO", "MAIL_TO"),
            Password = GetEnv("PASSWORD", "SMTP_PASSWORD", "EMAIL_PASSWORD", "MAIL_PASSWORD"),
            Username = GetEnv("USERNAME", "SMTP_USERNAME", "SMTP_USER", "SMTP_USER_NAME", "EMAIL_USERNAME", "EMAIL_USER", "MAIL_USERNAME", "MAIL_USER"),
            EnableSsl = GetEnvFlag(["ENABLE_SSL", "SMTP_ENABLE_SSL", "SMTP_SSL", "SMTP_USE_SSL", "SMTP_SSL_ENABLED", "EMAIL_ENABLE_SSL", "MAIL_ENABLE_SSL", "MAIL_SSL"], defaultValue: true)
        };
    }

    private static string GetEnv(params string[] names)
    {
        foreach (var name in names)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    private static bool GetEnvFlag(string[] names, bool defaultValue)
    {
        var value = GetEnv(names);
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (bool.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return NormalizeFlagValue(value) switch
        {
            "1" or "yes" or "sim" or "on" or "enabled" => true,
            "0" or "no" or "nao" or "off" or "disabled" => false,
            _ => defaultValue
        };
    }

    private static string NormalizeFlagValue(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var chars = decomposed
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);

        return string.Concat(chars).ToLowerInvariant();
    }
}
