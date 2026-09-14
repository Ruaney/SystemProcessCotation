namespace SystemProcessCotation.Tests;

public class CommandLineHelperTests
{
    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    [InlineData("/?")]
    public void IsHelpRequest_ReturnsTrueForHelpFlags(string flag)
    {
        Assert.True(global::CommandLineHelper.IsHelpRequest([flag]));
    }

    [Fact]
    public void IsHelpRequest_ReturnsFalseWhenHelpFlagIsMixedWithRunArguments()
    {
        Assert.False(global::CommandLineHelper.IsHelpRequest(["PETR4", "--help", "30.00"]));
    }

    [Fact]
    public void Usage_IncludesOptionalTimingArguments()
    {
        Assert.Contains("[intervaloMs]", global::CommandLineHelper.Usage);
        Assert.Contains("[cooldownSegundos]", global::CommandLineHelper.Usage);
        Assert.Contains("1s 1m", global::CommandLineHelper.Usage);
    }

    [Fact]
    public void ParseArguments_NormalizesSymbolAndPrices()
    {
        var settings = global::CommandLineHelper.ParseArguments([" petr4 ", "35.50", "30.25"]);

        Assert.Equal("PETR4", settings.StockSymbol);
        Assert.Equal(35.50, settings.PriceToSell);
        Assert.Equal(30.25, settings.PriceToBuy);
        Assert.Equal(3000, settings.CheckIntervalMs);
        Assert.Equal(60, settings.AlertCooldownSeconds);
    }

    [Fact]
    public void ParseArguments_AcceptsBrazilianDecimalSeparator()
    {
        var settings = global::CommandLineHelper.ParseArguments(["PETR4", "35,50", "30,25"]);

        Assert.Equal(35.50, settings.PriceToSell);
        Assert.Equal(30.25, settings.PriceToBuy);
    }

    [Fact]
    public void ParseArguments_AcceptsBrazilianCurrencyThresholds()
    {
        var settings = global::CommandLineHelper.ParseArguments(["PETR4", "R$ 1.234,56", "R$ 1.200,00"]);

        Assert.Equal(1234.56, settings.PriceToSell);
        Assert.Equal(1200.00, settings.PriceToBuy);
    }

    [Fact]
    public void ParseArguments_AcceptsOptionalTimingArguments()
    {
        var settings = global::CommandLineHelper.ParseArguments(["PETR4", "35.50", "30.25", "1000", "15"]);

        Assert.Equal(1000, settings.CheckIntervalMs);
        Assert.Equal(15, settings.AlertCooldownSeconds);
    }

    [Theory]
    [InlineData("750ms", "45s", 750, 45)]
    [InlineData("2s", "1m", 2000, 60)]
    public void ParseArguments_AcceptsDurationSuffixesForTimingArguments(
        string interval,
        string cooldown,
        int expectedInterval,
        int expectedCooldown)
    {
        var settings = global::CommandLineHelper.ParseArguments(["PETR4", "35.50", "30.25", interval, cooldown]);

        Assert.Equal(expectedInterval, settings.CheckIntervalMs);
        Assert.Equal(expectedCooldown, settings.AlertCooldownSeconds);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("fast")]
    public void ParseArguments_RejectsInvalidCheckInterval(string interval)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => global::CommandLineHelper.ParseArguments(["PETR4", "35.50", "30.25", interval]));

        Assert.Contains("intervalo de checagem", exception.Message);
    }

    [Fact]
    public void ParseArguments_RejectsMillisecondSuffixForCooldown()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => global::CommandLineHelper.ParseArguments(["PETR4", "35.50", "30.25", "1000", "500ms"]));

        Assert.Contains("cooldown de alerta", exception.Message);
    }

    [Fact]
    public void ParseArguments_TreatsSingleThreeDigitSeparatorAsThousands()
    {
        var settings = global::CommandLineHelper.ParseArguments(["PETR4", "1.234", "1,200"]);

        Assert.Equal(1234.00, settings.PriceToSell);
        Assert.Equal(1200.00, settings.PriceToBuy);
    }

    [Fact]
    public void ParseArguments_RejectsEmptySymbol()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => global::CommandLineHelper.ParseArguments([" ", "35.50", "30.25"]));

        Assert.Contains("ativo", exception.Message);
    }

    [Fact]
    public void ParseArguments_RejectsInvalidThresholdOrder()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => global::CommandLineHelper.ParseArguments(["PETR4", "30.00", "30.00"]));

        Assert.Contains("venda deve ser maior", exception.Message);
    }

    [Fact]
    public void ParseArguments_RejectsNonPositivePrices()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => global::CommandLineHelper.ParseArguments(["PETR4", "35.00", "0"]));

        Assert.Contains("maiores que zero", exception.Message);
    }

    [Fact]
    public void NormalizeAndValidate_NormalizesSymbolAndDefaultsTiming()
    {
        var settings = new global::TradingSettings
        {
            StockSymbol = " petr4 ",
            PriceToSell = 35.50,
            PriceToBuy = 30.25,
            CheckIntervalMs = 0,
            AlertCooldownSeconds = -1
        };

        settings.NormalizeAndValidate();

        Assert.Equal("PETR4", settings.StockSymbol);
        Assert.Equal(3000, settings.CheckIntervalMs);
        Assert.Equal(60, settings.AlertCooldownSeconds);
    }

    [Fact]
    public void NormalizeAndValidate_RejectsInvalidConfiguredThresholds()
    {
        var settings = new global::TradingSettings
        {
            StockSymbol = "PETR4",
            PriceToSell = 30.00,
            PriceToBuy = 30.00
        };

        var exception = Assert.Throws<ArgumentException>(() => settings.NormalizeAndValidate());

        Assert.Contains("venda deve ser maior", exception.Message);
    }

    [Theory]
    [InlineData("PETR 4")]
    [InlineData("PETR-4")]
    public void NormalizeAndValidate_RejectsSymbolsWithInvalidCharacters(string symbol)
    {
        var settings = new global::TradingSettings
        {
            StockSymbol = symbol,
            PriceToSell = 35.00,
            PriceToBuy = 30.00
        };

        var exception = Assert.Throws<ArgumentException>(() => settings.NormalizeAndValidate());

        Assert.Contains("letras e números", exception.Message);
    }

    [Theory]
    [InlineData(double.NaN, 30.00)]
    [InlineData(double.PositiveInfinity, 30.00)]
    [InlineData(35.00, double.NaN)]
    [InlineData(35.00, double.PositiveInfinity)]
    public void NormalizeAndValidate_RejectsNonFiniteThresholds(double sellPrice, double buyPrice)
    {
        var settings = new global::TradingSettings
        {
            StockSymbol = "PETR4",
            PriceToSell = sellPrice,
            PriceToBuy = buyPrice
        };

        var exception = Assert.Throws<ArgumentException>(() => settings.NormalizeAndValidate());

        Assert.Contains("finitos", exception.Message);
    }
}
