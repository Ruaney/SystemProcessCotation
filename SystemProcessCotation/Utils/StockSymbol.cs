public static class StockSymbol
{
    private const int MaxLength = 12;
    private static readonly string[] KnownPrefixes = ["B3:", "BVMF:", "BMFBOVESPA:"];

    public static bool TryNormalize(string? symbol, out string normalized)
    {
        normalized = StripKnownSuffix(StripKnownPrefix(symbol?.Trim().ToUpperInvariant() ?? string.Empty));
        return !string.IsNullOrWhiteSpace(normalized)
            && normalized.Length <= MaxLength
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

        if (normalized.Length > MaxLength)
        {
            throw new ArgumentException($"O código do ativo deve ter no máximo {MaxLength} caracteres.", paramName);
        }

        throw new ArgumentException("O código do ativo deve conter apenas letras e números.", paramName);
    }

    private static string StripKnownPrefix(string value)
    {
        foreach (var prefix in KnownPrefixes)
        {
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return value[prefix.Length..];
            }
        }

        return value;
    }

    private static string StripKnownSuffix(string value) =>
        value.EndsWith(".SA", StringComparison.OrdinalIgnoreCase)
            ? value[..^3]
            : value;

    private static bool IsTickerCharacter(char value) =>
        value is >= 'A' and <= 'Z' or >= '0' and <= '9';
}
