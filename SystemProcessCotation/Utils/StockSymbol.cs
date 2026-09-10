public static class StockSymbol
{
    public static string NormalizeOrThrow(string? symbol, string paramName = "symbol")
    {
        var normalized = symbol?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("O código do ativo é obrigatório.", paramName);
        }

        if (normalized.Any(c => !IsTickerCharacter(c)))
        {
            throw new ArgumentException("O código do ativo deve conter apenas letras e números.", paramName);
        }

        return normalized;
    }

    private static bool IsTickerCharacter(char value) =>
        value is >= 'A' and <= 'Z' or >= '0' and <= '9';
}
