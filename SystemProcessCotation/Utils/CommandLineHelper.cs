public static class CommandLineHelper
{
    public const string Usage = """
        Uso: dotnet run <Ativo> <precoVenda> <precoCompra> [intervaloMs] [cooldownSegundos]

        Exemplos:
          dotnet run PETR4 22.67 22.59
          dotnet run PETR4 22.67 22.59 1000 15
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
            settings.CheckIntervalMs = ParsePositiveInt(args[3], "intervalo de checagem");
        }

        if (args.Length >= 5)
        {
            settings.AlertCooldownSeconds = ParsePositiveInt(args[4], "cooldown de alerta");
        }

        return settings.NormalizeAndValidate();
    }

    private static int ParsePositiveInt(string value, string fieldName)
    {
        if (int.TryParse(value, out var parsed) && parsed > 0)
        {
            return parsed;
        }

        throw new ArgumentException($"O {fieldName} deve ser um número inteiro maior que zero.");
    }
}
