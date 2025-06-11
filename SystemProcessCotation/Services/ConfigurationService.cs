using DotNetEnv;

public class ConfigurationService : IConfigurationService
{
    private static ConfigurationService? _instance;
    private static readonly object _lock = new Object();
    private AppSettings? _appSettings;

    private ConfigurationService() { }

    public static ConfigurationService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new ConfigurationService();
                }
            }
            return _instance;
        }
    }

    public AppSettings GetAppSettings(TradingSettings tradingSettings)
    {
        if (_appSettings == null)
        {
            LoadEnvironmentVariable();
            _appSettings = new AppSettings
            {
                SmtpSettings = LoadSmtpSettings(),
                TradingSettings = tradingSettings
            };
        }
        else
        {
            _appSettings.TradingSettings = tradingSettings;
        }
        return _appSettings;
    }

    private void LoadEnvironmentVariable()
    {
        DotNetEnv.Env.Load("../../../.env");
    }
    private SmtpSettings LoadSmtpSettings()
    {
        return new SmtpSettings
        {
            Host = GetRequiredEnvironmentVariable("HOST"),
            Port = int.Parse(GetRequiredEnvironmentVariable("PORT")),
            FromAddress = GetRequiredEnvironmentVariable("FROM"),
            ToAddress = GetRequiredEnvironmentVariable("TO"),
            Password = GetRequiredEnvironmentVariable("PASSWORD"),
            Username = GetRequiredEnvironmentVariable("USERNAME"),
            EnableSsl = true
        };
    }

    private string GetRequiredEnvironmentVariable(string name)
    {
        return Environment.GetEnvironmentVariable(name) ?? throw new InvalidOperationException($"{name} variavel de ambiente não definida no arquivo de configuração .env");
    }
}