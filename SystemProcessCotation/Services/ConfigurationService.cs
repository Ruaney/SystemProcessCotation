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
            Port = int.TryParse(GetEnv("PORT"), out var port) ? port : 0,
            FromAddress = GetEnv("FROM"),
            ToAddress = GetEnv("TO"),
            Password = GetEnv("PASSWORD"),
            Username = GetEnv("USERNAME"),
            EnableSsl = true
        };
    }

    private static string GetEnv(string name) =>
        Environment.GetEnvironmentVariable(name) ?? string.Empty;
}
