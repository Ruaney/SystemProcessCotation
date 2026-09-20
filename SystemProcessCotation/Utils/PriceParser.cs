using System.Globalization;
using System.Text.RegularExpressions;

public static class PriceParser
{
    private static readonly Regex PriceTokenPattern = new(@"[-+]?\d+(?:[.,]\d+)*", RegexOptions.Compiled);
    private static readonly string[] CurrencyMarkers = ["R$", "BRL"];
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
        var compact = string.Concat((value ?? string.Empty).Where(c => !char.IsWhiteSpace(c)));
        var currencyPrice = ExtractCurrencyMarkedToken(compact);
        if (!string.IsNullOrEmpty(currencyPrice))
        {
            return currencyPrice;
        }

        var withoutCurrency = compact
            .Replace("R$", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("BRL", string.Empty, StringComparison.OrdinalIgnoreCase);

        return FirstPriceToken(withoutCurrency) ?? compact;
    }

    private static string? ExtractCurrencyMarkedToken(string value)
    {
        foreach (var marker in CurrencyMarkers)
        {
            var searchIndex = 0;
            while (searchIndex < value.Length)
            {
                var markerIndex = value.IndexOf(marker, searchIndex, StringComparison.OrdinalIgnoreCase);
                if (markerIndex < 0)
                {
                    break;
                }

                var afterMarker = value[(markerIndex + marker.Length)..];
                var tokenAfterMarker = LeadingPriceToken(afterMarker);
                if (!string.IsNullOrEmpty(tokenAfterMarker))
                {
                    return tokenAfterMarker;
                }

                var beforeMarker = value[..markerIndex];
                var tokenBeforeMarker = LastPriceToken(beforeMarker);
                if (!string.IsNullOrEmpty(tokenBeforeMarker))
                {
                    return tokenBeforeMarker;
                }

                searchIndex = markerIndex + marker.Length;
            }
        }

        return null;
    }

    private static string? FirstPriceToken(string value)
    {
        return PriceTokenPattern
            .Matches(value)
            .FirstOrDefault(match => IsPriceCandidate(value, match))
            ?.Value;
    }

    private static string? LeadingPriceToken(string value)
    {
        var candidate = value.TrimStart(':', '=');
        if (candidate.Length == 0 || !char.IsDigit(candidate[0]) && candidate[0] is not '-' and not '+')
        {
            return null;
        }

        var match = PriceTokenPattern.Match(candidate);
        return match.Success && match.Index == 0 && IsPriceCandidate(candidate, match)
            ? match.Value
            : null;
    }

    private static string? LastPriceToken(string value)
    {
        return PriceTokenPattern
            .Matches(value)
            .Cast<Match>()
            .LastOrDefault(match => IsPriceCandidate(value, match))
            ?.Value;
    }

    private static bool IsPriceCandidate(string value, Match match) =>
        !IsPercentageToken(value, match.Index + match.Length)
        && !IsDateToken(value, match.Index, match.Index + match.Length);

    private static bool IsPercentageToken(string value, int tokenEndIndex)
    {
        var nextNonWhitespace = value
            .Skip(tokenEndIndex)
            .FirstOrDefault(c => !char.IsWhiteSpace(c));

        return nextNonWhitespace == '%';
    }

    private static bool IsDateToken(string value, int tokenStartIndex, int tokenEndIndex)
    {
        var previous = tokenStartIndex > 0 ? value[tokenStartIndex - 1] : '\0';
        var next = tokenEndIndex < value.Length ? value[tokenEndIndex] : '\0';

        return previous == '/' || next == '/';
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

    private static bool LooksLikeBrazilianThousands(string value)
    {
        var decimalSeparatorIndex = value.LastIndexOf(',');
        var integerPart = decimalSeparatorIndex >= 0 ? value[..decimalSeparatorIndex] : value;
        var decimalPart = decimalSeparatorIndex >= 0 ? value[(decimalSeparatorIndex + 1)..] : string.Empty;

        if (integerPart.IndexOf('.') < 0)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(decimalPart) && !decimalPart.All(char.IsDigit))
        {
            return false;
        }

        var groups = integerPart.Split('.');
        return groups.Length > 1
            && groups[0].Length is >= 1 and <= 3
            && groups[0].All(char.IsDigit)
            && groups.Skip(1).All(group => group.Length == 3 && group.All(char.IsDigit));
    }
}
