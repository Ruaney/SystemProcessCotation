using HtmlAgilityPack;
using System.Globalization;
using System.Text;

public class CotationService : ICotationService
{
    private const string BrowserUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
    private const string PreferredLanguages = "pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7";

    private readonly HttpClient _httpClient;

    public CotationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CotationResult> GetCotationAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = StockSymbol.NormalizeOrThrow(symbol);

        try
        {
            var url = $"https://www.fundamentus.com.br/detalhes.php?papel={Uri.EscapeDataString(normalizedSymbol)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("User-Agent", BrowserUserAgent);
            request.Headers.TryAddWithoutValidation("Accept-Language", PreferredLanguages);

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var price = ExtractCotationPrice(doc);
            if (price > 0)
            {
                return new CotationResult
                {
                    Symbol = normalizedSymbol,
                    Price = price.Value,
                    Timestamp = DateTime.UtcNow
                };
            }

            throw new InvalidOperationException($"Não foi possível extrair a cotação para {normalizedSymbol}.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Erro ao buscar cotação para {normalizedSymbol}: {ex.Message}", ex);
        }
    }

    private static double? ExtractCotationPrice(HtmlDocument doc)
    {
        foreach (var text in ExtractCotationCandidates(doc))
        {
            if (PriceParser.TryParse(text, out var price) && price > 0)
            {
                return price;
            }
        }

        return null;
    }

    private static IEnumerable<string> ExtractCotationCandidates(HtmlDocument doc)
    {
        foreach (var labeledPrice in ExtractValuesAfterCotationLabels(doc))
        {
            yield return labeledPrice;
        }

        var highlightedPriceCells = doc.DocumentNode.SelectNodes(
            "//td[contains(concat(' ', normalize-space(@class), ' '), ' data ') " +
            "and contains(concat(' ', normalize-space(@class), ' '), ' destaque ') " +
            "and contains(concat(' ', normalize-space(@class), ' '), ' w3 ')]");

        if (highlightedPriceCells is null)
        {
            yield break;
        }

        foreach (var cell in highlightedPriceCells)
        {
            foreach (var text in ExtractPriceTexts(cell))
            {
                yield return text;
            }
        }
    }

    private static IEnumerable<string> ExtractValuesAfterCotationLabels(HtmlDocument doc)
    {
        var cells = doc.DocumentNode.SelectNodes("//td|//th");
        if (cells is null)
        {
            yield break;
        }

        foreach (var cell in cells)
        {
            if (HasCotationLabelText(cell))
            {
                foreach (var text in ExtractPriceTexts(cell))
                {
                    yield return text;
                }
            }

            if (!IsCotationLabel(cell.InnerText))
            {
                continue;
            }

            var valueCell = cell.SelectSingleNode("following-sibling::*[self::td or self::th][1]");
            foreach (var text in ExtractPriceTexts(valueCell))
            {
                yield return text;
            }
        }
    }

    private static bool HasCotationLabelText(HtmlNode cell) =>
        cell.DescendantsAndSelf()
            .Where(node => node.NodeType == HtmlNodeType.Text)
            .Any(node => IsCotationLabel(node.InnerText));

    private static IEnumerable<string> ExtractPriceTexts(HtmlNode? valueCell)
    {
        if (valueCell is null)
        {
            yield break;
        }

        var priceSpans = valueCell.SelectNodes(".//span[contains(concat(' ', normalize-space(@class), ' '), ' txt ')]");
        if (priceSpans is not null)
        {
            foreach (var span in priceSpans)
            {
                var spanText = span.InnerText.Trim();
                if (!string.IsNullOrWhiteSpace(spanText))
                {
                    yield return spanText;
                }
            }
        }

        var cellText = valueCell.InnerText.Trim();
        if (!string.IsNullOrWhiteSpace(cellText))
        {
            yield return cellText;
        }
    }

    private static bool IsCotationLabel(string value)
    {
        var normalized = NormalizeLabel(value);
        var withoutCurrencySuffix = RemoveCurrencySuffix(normalized);

        return IsKnownCotationLabel(normalized)
            || IsKnownCotationLabel(withoutCurrencySuffix);
    }

    private static bool IsKnownCotationLabel(string normalized) =>
        normalized is "cotacao"
            or "cotacaoatual"
            or "ultimacotacao"
            or "ultimopreco"
            or "ultimovalor"
            or "precoatual"
            or "valoratual";

    private static string RemoveCurrencySuffix(string value)
    {
        if (value.EndsWith("brl", StringComparison.Ordinal))
        {
            return value[..^3];
        }

        return value.EndsWith('r')
            ? value[..^1]
            : value;
    }

    private static string NormalizeLabel(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var chars = decomposed
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Where(char.IsLetterOrDigit);

        return string.Concat(chars).ToLowerInvariant();
    }
}
