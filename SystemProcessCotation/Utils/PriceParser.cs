using System.Globalization;
using System.Text.RegularExpressions;

public static class PriceParser
{
    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
    private static readonly Regex BrazilianRealMarker = new(@"R\s*\$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex BrazilianIsoMarker = new(@"BRL", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex NumericCandidate = new(@"[+-]?\d+(?:[.,]\d+)*", RegexOptions.CultureInvariant);
    private const NumberStyles PriceStyles = NumberStyles.Float | NumberStyles.AllowThousands;

    public static bool TryParse(string? value, out double price)
    {
        price = 0;

        var withoutCurrency = RemoveCurrencyMarkers(value);
        var normalizedValue = RemoveWhitespace(withoutCurrency);
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            return false;
        }

        if (TryParseNormalized(normalizedValue, out price))
        {
            return true;
        }

        foreach (var candidate in ExtractPriceCandidates(withoutCurrency))
        {
            if (TryParseNormalized(RemoveWhitespace(candidate), out price))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TryParseNormalized(string normalizedValue, out double price)
    {
        price = 0;

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

    private static string RemoveCurrencyMarkers(string? value)
    {
        var withoutRealMarker = BrazilianRealMarker.Replace(value ?? string.Empty, string.Empty);
        return BrazilianIsoMarker.Replace(withoutRealMarker, string.Empty);
    }

    private static string RemoveWhitespace(string value)
    {
        return string.Concat(value.Where(c => !char.IsWhiteSpace(c)));
    }

    private static IEnumerable<string> ExtractPriceCandidates(string value)
    {
        var candidates = NumericCandidate.Matches(value).Cast<Match>()
            .Where(match => !IsPercentageCandidate(value, match))
            .Select(match => match.Value)
            .ToArray();

        return candidates
            .Where(candidate => ContainsSeparator(candidate) && !StartsWithSign(candidate))
            .Concat(candidates.Where(candidate => ContainsSeparator(candidate) && StartsWithSign(candidate)))
            .Concat(candidates.Where(candidate => !ContainsSeparator(candidate)));
    }

    private static bool IsPercentageCandidate(string value, Match match)
    {
        var nextIndex = match.Index + match.Length;
        while (nextIndex < value.Length && char.IsWhiteSpace(value[nextIndex]))
        {
            nextIndex++;
        }

        return nextIndex < value.Length && value[nextIndex] == '%';
    }

    private static bool ContainsSeparator(string value)
    {
        return value.Contains('.') || value.Contains(',');
    }

    private static bool StartsWithSign(string value)
    {
        return value.StartsWith('+') || value.StartsWith('-');
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
