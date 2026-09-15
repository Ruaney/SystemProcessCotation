public static class StockSymbol
{
    private const int MaxLength = 12;

    public static bool TryNormalize(string? symbol, out string normalized)
    {
        normalized = symbol?.Trim().ToUpperInvariant() ?? string.Empty;
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

    private static bool IsTickerCharacter(char value) =>
        value is >= 'A' and <= 'Z' or >= '0' and <= '9';
}
