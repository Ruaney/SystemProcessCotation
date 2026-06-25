using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Segredos de SMTP continuam vindo do .env (quando presente).
        if (File.Exists(".env"))
        {
            DotNetEnv.Env.Load();
        }

        var builder = Host.CreateApplicationBuilder();

        // Ativo/preços: argumentos de linha de comando têm prioridade (compatibilidade
        // com `dotnet run PETR4 22.67 22.59`); senão, lê a seção "Trading".
        var tradingSettings = ResolveTradingSettings(args, builder.Configuration);
        builder.Services.AddSingleton(tradingSettings);

        // SMTP opcional: ausente => NotificationWorker apenas registra os alertas.
        builder.Services.AddSingleton(new ConfigurationService().LoadSmtpSettings());

        // Conexão única com o Redis (barramento + estado dos alertas).
        var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(redisConnectionString);
            options.AbortOnConnectFail = false; // tolera o Redis subir depois (docker compose).
            return ConnectionMultiplexer.Connect(options);
        });

        builder.Services.AddSingleton<IEventBus, RedisEventBus>();
        builder.Services.AddSingleton<IAlertStateStore, RedisAlertStateStore>();
        builder.Services.AddSingleton<ITradingService, TradingService>();
        builder.Services.AddSingleton<IEmailService, EmailService>();
        builder.Services.AddHttpClient<ICotationService, CotationService>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        });

        builder.Services.AddHostedService<CotationWorker>();
        builder.Services.AddHostedService<TradingWorker>();
        builder.Services.AddHostedService<NotificationWorker>();

        var host = builder.Build();
        await host.RunAsync();
    }

    private static TradingSettings ResolveTradingSettings(string[] args, IConfiguration configuration)
    {
        if (args.Length == 3)
        {
            return CommandLineHelper.ParseArguments(args);
        }

        var section = configuration.GetSection("Trading");
        return new TradingSettings
        {
            StockSymbol = (section.GetValue<string>("StockSymbol") ?? "PETR4").ToUpperInvariant(),
            PriceToSell = section.GetValue<double>("PriceToSell"),
            PriceToBuy = section.GetValue<double>("PriceToBuy"),
            CheckIntervalMs = section.GetValue<int>("CheckIntervalMs")
        };
    }
}
