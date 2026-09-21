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
        if (LooksLikeBrazilianThousands(value))
        {
            return [BrazilianCulture, InvariantCulture];
        }

        var lastComma = value.LastIndexOf(',');
        var lastDot = value.LastIndexOf('.');

        if (LooksLikeSingleThousandsSeparator(value, '.', lastDot, lastComma))
        {
            return [BrazilianCulture, InvariantCulture];
        }

        if (LooksLikeSingleThousandsSeparator(value, ',', lastComma, lastDot))
        {
            return [InvariantCulture, BrazilianCulture];
        }

        var prefersBrazilian = lastComma >= 0 && lastComma > lastDot;

        return prefersBrazilian
            ? [BrazilianCulture, InvariantCulture]
            : [InvariantCulture, BrazilianCulture];
    }

    private static bool LooksLikeBrazilianThousands(string value)
    {
        var commaIndex = value.LastIndexOf(',');
        if (commaIndex <= 0)
        {
            return false;
        }

        var integerPart = value[..commaIndex];
        var decimalPart = value[(commaIndex + 1)..];
        if (decimalPart.Length is 0 or > 2 || !decimalPart.All(char.IsDigit))
        {
            return false;
        }

        var groups = integerPart.Split('.');
        return groups.Length > 1
            && groups[0].Length is >= 1 and <= 3
            && groups.All(group => group.All(char.IsDigit))
            && groups.Skip(1).All(group => group.Length == 3);
    }

    private static bool LooksLikeSingleThousandsSeparator(string value, char separator, int separatorIndex, int otherSeparatorIndex)
    {
        if (separatorIndex <= 0 || otherSeparatorIndex >= 0 || value.IndexOf(separator) != separatorIndex)
        {
            return false;
        }

        var digitsAfterSeparator = value.Length - separatorIndex - 1;
        return digitsAfterSeparator == 3
            && value[..separatorIndex].All(char.IsDigit)
            && value[(separatorIndex + 1)..].All(char.IsDigit);
    }
}
