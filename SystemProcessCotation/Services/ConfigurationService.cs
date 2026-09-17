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
            Host = GetEnv("HOST", "SMTP_HOST", "SMTP_SERVER", "EMAIL_HOST", "EMAIL_SERVER", "MAIL_HOST", "MAIL_SERVER"),
            Port = int.TryParse(GetEnv("PORT", "SMTP_PORT", "EMAIL_PORT", "MAIL_PORT"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var port) ? port : 0,
            FromAddress = GetEnv("FROM", "SMTP_FROM", "SMTP_FROM_ADDRESS", "SMTP_FROM_EMAIL", "EMAIL_FROM", "EMAIL_FROM_ADDRESS", "MAIL_FROM", "MAIL_FROM_ADDRESS"),
            ToAddress = GetEnv("TO", "SMTP_TO", "SMTP_TO_ADDRESS", "SMTP_TO_EMAIL", "EMAIL_TO", "EMAIL_TO_ADDRESS", "MAIL_TO", "MAIL_TO_ADDRESS"),
            Password = GetEnv("PASSWORD", "SMTP_PASSWORD", "EMAIL_PASSWORD", "MAIL_PASSWORD"),
            Username = GetEnv("USERNAME", "SMTP_USERNAME", "SMTP_USER", "SMTP_USER_NAME", "SMTP_USER_EMAIL", "EMAIL_USERNAME", "EMAIL_USER", "EMAIL_USER_NAME", "EMAIL_USER_EMAIL", "MAIL_USERNAME", "MAIL_USER", "MAIL_USER_NAME", "MAIL_USER_EMAIL"),
            EnableSsl = GetEnvFlag(["ENABLE_SSL", "SMTP_ENABLE_SSL", "SMTP_SSL", "SMTP_USE_SSL", "SMTP_SSL_ENABLED", "EMAIL_ENABLE_SSL", "EMAIL_SSL", "EMAIL_USE_SSL", "MAIL_ENABLE_SSL", "MAIL_SSL", "MAIL_USE_SSL", "MAIL_SSL_ENABLED"], defaultValue: true)
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
            "1" or "yes" or "sim" or "on" or "enabled" or "verdadeiro" or "ligado" or "habilitado" => true,
            "0" or "no" or "nao" or "off" or "disabled" or "falso" or "desligado" or "desabilitado" => false,
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
