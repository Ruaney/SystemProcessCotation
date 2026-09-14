using System.Globalization;

public static class PriceParser
{
    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
    private const NumberStyles PriceStyles = NumberStyles.Float | NumberStyles.AllowThousands;

    public static bool TryParse(string? value, out double price)
    {
        price = 0;

        var normalizedValue = Normalize(value);
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            return false;
        }

        foreach (var culture in PreferredCultures(normalizedValue))
        {
            if (decimal.TryParse(normalizedValue, PriceStyles, culture, out var parsedPrice))
            {
                price = (double)parsedPrice;
                return true;
            }
        }

        return false;
    }

    private static string Normalize(string? value) =>
        (value ?? string.Empty)
            .Replace("R$", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("\u00a0", string.Empty)
            .Trim();

    private static CultureInfo[] PreferredCultures(string value)
    {
        if (LooksLikeBrazilianThousands(value))
        {
            return [BrazilianCulture, InvariantCulture];
        }

        var lastComma = value.LastIndexOf(',');
        var lastDot = value.LastIndexOf('.');
        var prefersBrazilian = lastComma >= 0 && lastComma > lastDot;

        return prefersBrazilian
            ? [BrazilianCulture, InvariantCulture]
            : [InvariantCulture, BrazilianCulture];
    }

    private static bool LooksLikeBrazilianThousands(string value)
    {
        if (value.Contains(','))
        {
            return false;
        }

        var unsignedValue = value.TrimStart('+', '-');
        var groups = unsignedValue.Split('.');

        return groups.Length > 1
            && groups[0].Length is > 0 and <= 3
            && groups.All(group => group.All(char.IsDigit))
            && groups.Skip(1).All(group => group.Length == 3);
    }
}
