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

    private static string Normalize(string? value)
    {
        var withoutCurrency = (value ?? string.Empty)
            .Replace("R$", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("BRL", string.Empty, StringComparison.OrdinalIgnoreCase);

        return string.Concat(withoutCurrency.Where(c => !char.IsWhiteSpace(c)));
    }

    private static CultureInfo[] PreferredCultures(string value)
    {
        var lastComma = value.LastIndexOf(',');
        var lastDot = value.LastIndexOf('.');
        var prefersBrazilian = lastComma >= 0 && lastComma > lastDot;

        return prefersBrazilian
            ? [BrazilianCulture, InvariantCulture]
            : [InvariantCulture, BrazilianCulture];
    }
}
