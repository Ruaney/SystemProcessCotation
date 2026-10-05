public static class StockSymbol
{
    private static readonly string[] B3ProviderPrefixes =
    [
        "B3",
        "BVMF",
        "BOVESPA",
        "BMFBOVESPA"
    ];
    private static readonly string[] B3ProviderSuffixes =
    [
        ".SA",
        ".B3",
        ".BVMF",
        ".BOVESPA",
        ".BMFBOVESPA"
    ];

    public static bool TryNormalize(string? symbol, out string normalized)
    {
        normalized = RemoveB3Affixes(symbol?.Trim().ToUpperInvariant() ?? string.Empty);
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

    private static string RemoveB3Affixes(string symbol) =>
        RemoveB3Suffix(RemoveB3Prefix(symbol));

    private static string RemoveB3Prefix(string symbol)
    {
        var separatorIndex = symbol.IndexOf(':');
        if (separatorIndex < 0)
        {
            return symbol;
        }

        var provider = symbol[..separatorIndex].Trim();
        return B3ProviderPrefixes.Contains(provider, StringComparer.Ordinal)
            ? symbol[(separatorIndex + 1)..].Trim()
            : symbol;
    }

    private static string RemoveB3Suffix(string symbol)
    {
        foreach (var suffix in B3ProviderSuffixes)
        {
            if (symbol.EndsWith(suffix, StringComparison.Ordinal))
            {
                return symbol[..^suffix.Length];
            }
        }

        return symbol;
    }
}
