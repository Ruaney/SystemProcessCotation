using System.Globalization;

public static class CommandLineHelper
{
    public const string Usage = """
        Uso: dotnet run <Ativo> <precoVenda> <precoCompra> [intervaloMs] [cooldownSegundos]

        Exemplos:
          dotnet run PETR4 22.67 22.59
          dotnet run PETR4 22.67 22.59 1000 15
          dotnet run PETR4 22.67 22.59 1s 1m
        """;

    public static bool IsHelpRequest(string[] args)
    {
        if (args.Length != 1)
        {
            return false;
        }

        var option = args[0].Trim();
        return option.Equals("-h", StringComparison.OrdinalIgnoreCase)
            || option.Equals("--help", StringComparison.OrdinalIgnoreCase)
            || option.Equals("/?", StringComparison.OrdinalIgnoreCase);
    }

    public static TradingSettings ParseArguments(string[] args)
    {
        if (args.Length is < 3 or > 5)
        {
            throw new ArgumentException("Número incorreto de parâmetros. Esperado: 3 a 5 parâmetros. Use --help para ver exemplos.");
        }

        if (!PriceParser.TryParse(args[1], out var sellPrice))
        {
            throw new ArgumentException($"Preço de venda inválido: {args[1]}. Use formato decimal com ponto ou vírgula (ex: 22.67 ou 22,67)");
        }

        if (!PriceParser.TryParse(args[2], out var buyPrice))
        {
            throw new ArgumentException($"Preço de compra inválido: {args[2]}. Use formato decimal com ponto ou vírgula (ex: 22.67 ou 22,67)");
        }

        var settings = new TradingSettings
        {
            StockSymbol = args[0],
            PriceToSell = sellPrice,
            PriceToBuy = buyPrice
        };

        if (args.Length >= 4)
        {
            settings.CheckIntervalMs = ParseCheckInterval(args[3]);
        }

        if (args.Length >= 5)
        {
            settings.AlertCooldownSeconds = ParseCooldownSeconds(args[4]);
        }

        return settings.NormalizeAndValidate();
    }

    private static int ParseCheckInterval(string value) =>
        ParsePositiveDuration(
            value,
            "intervalo de checagem",
            1,
            ("ms", 1),
            ("s", 1000),
            ("m", 60000));

    private static int ParseCooldownSeconds(string value) =>
        ParsePositiveDuration(
            value,
            "cooldown de alerta",
            1,
            ("s", 1),
            ("m", 60));

    private static int ParsePositiveDuration(
        string value,
        string fieldName,
        int defaultUnitMultiplier,
        params (string Suffix, int Multiplier)[] suffixes)
    {
        var numberText = value.Trim();
        var multiplier = defaultUnitMultiplier;

        foreach (var (suffix, suffixMultiplier) in suffixes.OrderByDescending(item => item.Suffix.Length))
        {
            if (!numberText.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            multiplier = suffixMultiplier;
            numberText = numberText[..^suffix.Length].Trim();
            break;
        }

        if (!int.TryParse(numberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
        {
            throw new ArgumentException($"O {fieldName} deve ser um número inteiro maior que zero.");
        }

        try
        {
            return checked(parsed * multiplier);
        }
        catch (OverflowException ex)
        {
            throw new ArgumentException($"O {fieldName} deve caber em um número inteiro.", ex);
        }
    }
}
