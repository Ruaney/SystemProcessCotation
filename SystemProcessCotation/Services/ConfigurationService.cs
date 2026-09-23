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
            Host = GetEnv("HOST", "SMTP_HOST"),
            Port = GetEnvInt("PORT", "SMTP_PORT"),
            FromAddress = GetEnv("FROM", "SMTP_FROM"),
            ToAddress = GetEnv("TO", "SMTP_TO"),
            Password = GetEnv("PASSWORD", "SMTP_PASSWORD"),
            Username = GetEnv("USERNAME", "SMTP_USERNAME", "SMTP_USER"),
            EnableSsl = GetEnvFlag(["ENABLE_SSL", "SMTP_ENABLE_SSL", "SMTP_SSL"], defaultValue: true)
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

    private static int GetEnvInt(params string[] names)
    {
        foreach (var name in names)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }

        return 0;
    }

    private static bool GetEnvFlag(string[] names, bool defaultValue)
    {
        foreach (var name in names)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (TryParseEnvFlag(value, out var parsed))
            {
                return parsed;
            }
        }

        return defaultValue;
    }

    private static bool TryParseEnvFlag(string value, out bool parsed)
    {
        if (bool.TryParse(value, out parsed))
        {
            return true;
        }

        switch (NormalizeFlagValue(value))
        {
            case "1" or "yes" or "sim" or "on" or "enabled":
                parsed = true;
                return true;
            case "0" or "no" or "nao" or "off" or "disabled":
                parsed = false;
                return true;
            default:
                parsed = false;
                return false;
        }
    }

    private static string NormalizeFlagValue(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var chars = decomposed
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);

        return string.Concat(chars).ToLowerInvariant();
    }
}
