using System.Globalization;
using System.Text.RegularExpressions;

public static class PriceParser
{
    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
    private static readonly Regex BrazilianRealMarker = new(@"R\s*\$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex BrazilianIsoMarker = new(@"BRL", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    private static readonly Regex NumericCandidate = new(@"[+-]?\d+(?:(?:[.,]\d+)|(?:\s+\d{3}))*", RegexOptions.CultureInvariant);
    private static readonly Regex DottedDateCandidate = new(@"^\d{1,2}\.\d{1,2}\.\d{2,4}$", RegexOptions.CultureInvariant);
    private static readonly Regex DottedDateText = new(@"\d{1,2}\s*\.\s*\d{1,2}\s*\.\s*\d{2,4}", RegexOptions.CultureInvariant);
    private const NumberStyles PriceStyles = NumberStyles.Float | NumberStyles.AllowThousands;

    public static bool TryParse(string? value, out double price)
    {
        price = 0;

        var withoutCurrency = NormalizeMinusSigns(RemoveCurrencyMarkers(value));
        var normalizedValue = RemoveWhitespace(withoutCurrency);
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            return false;
        }

        if (!ContainsDottedDate(withoutCurrency) && TryParseNormalized(normalizedValue, out price))
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

    private static string NormalizeMinusSigns(string value) =>
        value
            .Replace('\u2212', '-')
            .Replace('\uFE63', '-')
            .Replace('\uFF0D', '-');

    private static string RemoveWhitespace(string value)
    {
        return string.Concat(value.Where(c => !char.IsWhiteSpace(c)));
    }

    private static IEnumerable<string> ExtractPriceCandidates(string value)
    {
        var searchableValue = MaskDottedDates(value);
        var candidates = NumericCandidate.Matches(searchableValue).Cast<Match>()
            .Where(match => !IsPercentageCandidate(searchableValue, match))
            .Where(match => !IsDateCandidate(searchableValue, match))
            .Where(match => !IsTimeCandidate(searchableValue, match))
            .Where(match => ContainsSeparator(match.Value) || !IsEmbeddedInWord(searchableValue, match))
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

    private static bool IsDateCandidate(string value, Match match)
    {
        return HasAdjacentDateSeparator(value, match.Index - 1, -1)
            || HasAdjacentDateSeparator(value, match.Index + match.Length, 1)
            || StartsWithDateSeparator(value, match)
            || LooksLikeDottedDate(RemoveWhitespace(match.Value));
    }

    private static bool HasAdjacentDateSeparator(string value, int index, int step)
    {
        while (index >= 0 && index < value.Length && char.IsWhiteSpace(value[index]))
        {
            index += step;
        }

        return index >= 0 && index < value.Length && IsDateSeparator(value[index]);
    }

    private static bool StartsWithDateSeparator(string value, Match match)
    {
        return match.Value.Length > 1
            && IsDateSeparator(match.Value[0])
            && match.Index > 0
            && char.IsDigit(value[match.Index - 1]);
    }

    private static bool IsDateSeparator(char value)
    {
        return value is '/' or '-' or '.';
    }

    private static bool LooksLikeDottedDate(string value) =>
        DottedDateCandidate.IsMatch(value);

    private static bool ContainsDottedDate(string value) =>
        DottedDateText.IsMatch(value);

    private static string MaskDottedDates(string value) =>
        DottedDateText.Replace(value, match => new string(' ', match.Length));

    private static bool IsTimeCandidate(string value, Match match)
    {
        if (ContainsSeparator(match.Value))
        {
            return false;
        }

        return HasAdjacentTimeSeparator(value, match.Index - 1, -1)
            || HasAdjacentTimeSeparator(value, match.Index + match.Length, 1);
    }

    private static bool HasAdjacentTimeSeparator(string value, int index, int step)
    {
        while (index >= 0 && index < value.Length && char.IsWhiteSpace(value[index]))
        {
            index += step;
        }

        return index >= 0 && index < value.Length && value[index] == ':';
    }

    private static bool IsEmbeddedInWord(string value, Match match)
    {
        return HasAdjacentLetter(value, match.Index - 1)
            || HasAdjacentLetter(value, match.Index + match.Length);
    }

    private static bool HasAdjacentLetter(string value, int index)
    {
        return index >= 0
            && index < value.Length
            && char.IsLetter(value[index]);
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
