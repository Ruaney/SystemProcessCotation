public static class StockSymbol
{
    private const string B3ExchangeSuffix = ".SA";

    public static bool TryNormalize(string? symbol, out string normalized)
    {
        normalized = symbol?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalized.EndsWith(B3ExchangeSuffix, StringComparison.Ordinal))
        {
            normalized = normalized[..^B3ExchangeSuffix.Length];
        }

        return !string.IsNullOrWhiteSpace(normalized)
            && normalized.All(IsTickerCharacter);
    }

    public static string NormalizeOrThrow(string? symbol, string paramName = "symbol")
    {
        if (TryNormalize(symbol, out var normalized))
        {
            return normalized;
        }

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("O código do ativo é obrigatório.", paramName);
        }

        throw new ArgumentException("O código do ativo deve conter apenas letras e números.", paramName);
    }

    private static bool IsTickerCharacter(char value) =>
        value is >= 'A' and <= 'Z' or >= '0' and <= '9';
}
