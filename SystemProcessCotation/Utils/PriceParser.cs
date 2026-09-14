using System.Globalization;
using System.Text.RegularExpressions;

public static class PriceParser
{
    private static readonly Regex PriceTokenPattern = new(@"\d+(?:[.,]\d+)*", RegexOptions.Compiled);
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

        var compact = string.Concat(withoutCurrency.Where(c => !char.IsWhiteSpace(c)));
        var match = PriceTokenPattern.Match(compact);

        return match.Success ? match.Value : compact;
    }

    private static CultureInfo[] PreferredCultures(string value)
    {
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
