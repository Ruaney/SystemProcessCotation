using System.Globalization;

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

        return value.ToLowerInvariant() switch
        {
            "1" or "yes" or "sim" => true,
            "0" or "no" or "nao" => false,
            _ => defaultValue
        };
    }
}
