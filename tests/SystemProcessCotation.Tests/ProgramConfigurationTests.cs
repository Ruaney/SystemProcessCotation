using Microsoft.Extensions.Configuration;

namespace SystemProcessCotation.Tests;

public class ProgramConfigurationTests
{
    [Fact]
    public void ResolveTradingSettings_UsesTradingSectionWhenArgumentsAreMissing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Trading:StockSymbol"] = " petr4.sa ",
            ["Trading:PriceToSell"] = "35.50",
            ["Trading:PriceToBuy"] = "30.25",
            ["Trading:CheckIntervalMs"] = "1000",
            ["Trading:AlertCooldownSeconds"] = "15"
        });

        var settings = global::Program.ResolveTradingSettings([], configuration);

        Assert.Equal("PETR4", settings.StockSymbol);
        Assert.Equal(35.50, settings.PriceToSell);
        Assert.Equal(30.25, settings.PriceToBuy);
        Assert.Equal(1000, settings.CheckIntervalMs);
        Assert.Equal(15, settings.AlertCooldownSeconds);
    }

    [Fact]
    public void ResolveTradingSettings_PrefersCommandLineArgumentsOverConfiguration()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Trading:StockSymbol"] = "VALE3",
            ["Trading:PriceToSell"] = "100.00",
            ["Trading:PriceToBuy"] = "90.00"
        });

        var settings = global::Program.ResolveTradingSettings(["petr4.sa", "35.50", "30.25", "1000", "15"], configuration);

        Assert.Equal("PETR4", settings.StockSymbol);
        Assert.Equal(35.50, settings.PriceToSell);
        Assert.Equal(30.25, settings.PriceToBuy);
        Assert.Equal(1000, settings.CheckIntervalMs);
        Assert.Equal(15, settings.AlertCooldownSeconds);
    }

    [Fact]
    public void ResolveTradingSettings_AcceptsLocalizedTradingThresholds()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Trading:StockSymbol"] = "PETR4",
            ["Trading:PriceToSell"] = "R$ 1.234,56",
            ["Trading:PriceToBuy"] = "BRL 1 200,00",
            ["Trading:CheckIntervalMs"] = "1000",
            ["Trading:AlertCooldownSeconds"] = "15"
        });

        var settings = global::Program.ResolveTradingSettings([], configuration);

        Assert.Equal(1234.56, settings.PriceToSell);
        Assert.Equal(1200.00, settings.PriceToBuy);
    }

    [Fact]
    public void ResolveTradingSettings_RejectsInvalidConfiguredThreshold()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Trading:StockSymbol"] = "PETR4",
            ["Trading:PriceToSell"] = "fast",
            ["Trading:PriceToBuy"] = "30.25"
        });

        var exception = Assert.Throws<ArgumentException>(() =>
            global::Program.ResolveTradingSettings([], configuration));

        Assert.Contains("preço de venda", exception.Message);
    }

    [Fact]
    public void ResolveTradingSettings_UsesTradingAliasesWhenSectionIsMissing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["STOCK_SYMBOL"] = " vale3.sa ",
            ["PRICE_TO_SELL"] = "R$ 70,00",
            ["PRICE_TO_BUY"] = "R$ 60,00"
        });

        var settings = global::Program.ResolveTradingSettings([], configuration);

        Assert.Equal("VALE3", settings.StockSymbol);
        Assert.Equal(70.00, settings.PriceToSell);
        Assert.Equal(60.00, settings.PriceToBuy);
    }

    [Fact]
    public void ResolveTradingSettings_UsesSchedulerFriendlyAliasesWhenSectionIsMissing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["TICKER"] = " itub4.sa ",
            ["SELL_THRESHOLD"] = "R$ 35,50",
            ["BUY_THRESHOLD"] = "R$ 30,25"
        });

        var settings = global::Program.ResolveTradingSettings([], configuration);

        Assert.Equal("ITUB4", settings.StockSymbol);
        Assert.Equal(35.50, settings.PriceToSell);
        Assert.Equal(30.25, settings.PriceToBuy);
    }

    [Fact]
    public void ResolveTradingSettings_PrefersTradingSectionOverAliases()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Trading:StockSymbol"] = "PETR4",
            ["Trading:PriceToSell"] = "35.50",
            ["Trading:PriceToBuy"] = "30.25",
            ["STOCK_SYMBOL"] = "VALE3",
            ["PRICE_TO_SELL"] = "70.00",
            ["PRICE_TO_BUY"] = "60.00"
        });

        var settings = global::Program.ResolveTradingSettings([], configuration);

        Assert.Equal("PETR4", settings.StockSymbol);
        Assert.Equal(35.50, settings.PriceToSell);
        Assert.Equal(30.25, settings.PriceToBuy);
    }

    [Fact]
    public void ResolveTradingSettings_UsesTimingAliasesWhenSectionIsMissing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["STOCK_SYMBOL"] = "PETR4",
            ["PRICE_TO_SELL"] = "35.50",
            ["PRICE_TO_BUY"] = "30.25",
            ["CHECK_INTERVAL_MS"] = "1500",
            ["ALERT_COOLDOWN_SECONDS"] = "20"
        });

        var settings = global::Program.ResolveTradingSettings([], configuration);

        Assert.Equal(1500, settings.CheckIntervalMs);
        Assert.Equal(20, settings.AlertCooldownSeconds);
    }

    [Fact]
    public void ResolveTradingSettings_UsesSchedulerFriendlyTimingAliases()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ASSET_TICKER"] = "PETR4",
            ["SELL_TARGET_PRICE"] = "35.50",
            ["BUY_TARGET_PRICE"] = "30.25",
            ["POLLING_INTERVAL_MS"] = "2500",
            ["COOLDOWN_SECONDS"] = "45"
        });

        var settings = global::Program.ResolveTradingSettings([], configuration);

        Assert.Equal(2500, settings.CheckIntervalMs);
        Assert.Equal(45, settings.AlertCooldownSeconds);
    }

    [Fact]
    public void ResolveTradingSettings_PrefersTradingTimingSectionOverAliases()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Trading:StockSymbol"] = "PETR4",
            ["Trading:PriceToSell"] = "35.50",
            ["Trading:PriceToBuy"] = "30.25",
            ["Trading:CheckIntervalMs"] = "1000",
            ["Trading:AlertCooldownSeconds"] = "15",
            ["CHECK_INTERVAL_MS"] = "1500",
            ["ALERT_COOLDOWN_SECONDS"] = "20"
        });

        var settings = global::Program.ResolveTradingSettings([], configuration);

        Assert.Equal(1000, settings.CheckIntervalMs);
        Assert.Equal(15, settings.AlertCooldownSeconds);
    }

    [Fact]
    public void ResolveTradingSettings_RejectsInvalidTimingAlias()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["STOCK_SYMBOL"] = "PETR4",
            ["PRICE_TO_SELL"] = "35.50",
            ["PRICE_TO_BUY"] = "30.25",
            ["CHECK_INTERVAL_MS"] = "fast"
        });

        var exception = Assert.Throws<ArgumentException>(() =>
            global::Program.ResolveTradingSettings([], configuration));

        Assert.Contains("intervalo de checagem", exception.Message);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void ResolveTradingSettings_RejectsNonPositiveTimingAlias(string interval)
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["STOCK_SYMBOL"] = "PETR4",
            ["PRICE_TO_SELL"] = "35.50",
            ["PRICE_TO_BUY"] = "30.25",
            ["CHECK_INTERVAL_MS"] = interval
        });

        var exception = Assert.Throws<ArgumentException>(() =>
            global::Program.ResolveTradingSettings([], configuration));

        Assert.Contains("maior que zero", exception.Message);
    }

    [Fact]
    public void ResolveAwsServiceUrl_UsesEndpointAliasWhenSectionIsMissing()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["AWS_ENDPOINT_URL"] = " http://localhost:4566 "
        });

        var serviceUrl = global::Program.ResolveAwsServiceUrl(configuration);

        Assert.Equal("http://localhost:4566", serviceUrl);
    }

    [Fact]
    public void ResolveAwsServiceUrl_PrefersAppSettingsSectionOverAliases()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Aws:ServiceUrl"] = "http://localstack:4566",
            ["AWS_ENDPOINT_URL"] = "http://localhost:4566"
        });

        var serviceUrl = global::Program.ResolveAwsServiceUrl(configuration);

        Assert.Equal("http://localstack:4566", serviceUrl);
    }

    [Fact]
    public void ResolveAwsServiceUrl_UnquotesConfiguredValue()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["AWS_ENDPOINT_URL"] = " \"http://localhost:4566\" "
        });

        var serviceUrl = global::Program.ResolveAwsServiceUrl(configuration);

        Assert.Equal("http://localhost:4566", serviceUrl);
    }

    [Fact]
    public void ResolveAwsServiceUrl_IgnoresEnvironmentStyleInlineComment()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["AWS_ENDPOINT_URL"] = " http://localhost:4566 # LocalStack "
        });

        var serviceUrl = global::Program.ResolveAwsServiceUrl(configuration);

        Assert.Equal("http://localhost:4566", serviceUrl);
    }

    [Fact]
    public void ResolveAwsRegion_UsesDefaultRegionAlias()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["AWS_DEFAULT_REGION"] = " sa-east-1 "
        });

        var region = global::Program.ResolveAwsRegion(configuration);

        Assert.Equal("sa-east-1", region);
    }

    [Fact]
    public void ResolveAwsRegion_DefaultsToUsEastWhenNoRegionIsConfigured()
    {
        var region = global::Program.ResolveAwsRegion(BuildConfiguration());

        Assert.Equal("us-east-1", region);
    }

    [Fact]
    public void LocalStackCredentials_UsesStandardAwsEnvironmentAliases()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["AWS_ACCESS_KEY_ID"] = " local-key ",
            ["AWS_SECRET_ACCESS_KEY"] = " local-secret "
        });

        var credentials = global::Program.LocalStackCredentials(configuration).GetCredentials();

        Assert.Equal("local-key", credentials.AccessKey);
        Assert.Equal("local-secret", credentials.SecretKey);
    }

    [Fact]
    public void ResolveRedisConnectionString_UsesDeploymentAlias()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["REDIS_CONNECTION_STRING"] = " redis:6379 "
        });

        var connectionString = global::Program.ResolveRedisConnectionString(configuration);

        Assert.Equal("redis:6379", connectionString);
    }

    [Fact]
    public void ResolveRedisConnectionString_UsesRedisHostAlias()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["REDIS_HOST"] = " cache "
        });

        var connectionString = global::Program.ResolveRedisConnectionString(configuration);

        Assert.Equal("cache", connectionString);
    }

    [Fact]
    public void ResolveRedisConnectionString_IgnoresQuotedBlankAlias()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["REDIS_CONNECTION_STRING"] = "' '",
            ["REDIS_URL"] = "redis:6379"
        });

        var connectionString = global::Program.ResolveRedisConnectionString(configuration);

        Assert.Equal("redis:6379", connectionString);
    }

    [Fact]
    public void ResolveRedisConnectionString_DefaultsToLocalhostWhenUnset()
    {
        var connectionString = global::Program.ResolveRedisConnectionString(BuildConfiguration());

        Assert.Equal("localhost:6379", connectionString);
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?>? values = null)
    {
        var builder = new ConfigurationBuilder();
        if (values is not null)
        {
            builder.AddInMemoryCollection(values);
        }

        return builder.Build();
    }
}
