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
            Host = GetEnv("HOST"),
            Port = int.TryParse(GetEnv("PORT"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var port) ? port : 0,
            FromAddress = GetEnv("FROM"),
            ToAddress = GetEnv("TO"),
            Password = GetEnv("PASSWORD"),
            Username = GetEnv("USERNAME"),
            EnableSsl = GetEnvFlag("ENABLE_SSL", defaultValue: true)
        };
    }

    private static string GetEnv(string name) =>
        (Environment.GetEnvironmentVariable(name) ?? string.Empty).Trim();

    private static bool GetEnvFlag(string name, bool defaultValue)
    {
        var value = GetEnv(name);
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
