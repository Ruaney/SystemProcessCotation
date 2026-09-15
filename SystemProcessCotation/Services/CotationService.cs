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

        var selectors = new[]
        {
            "//table[1]//td[contains(@class,'data') and contains(@class,'destaque') and contains(@class,'w3')]//span[contains(@class,'txt')]"
        };

        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode(selector);
            var text = node?.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return text;
            }
        }
    }

    private static IEnumerable<string> ExtractValuesAfterCotationLabels(HtmlDocument doc)
    {
        var cells = doc.DocumentNode.SelectNodes("//td");
        if (cells is null)
        {
            yield break;
        }

        foreach (var cell in cells)
        {
            if (!IsCotationLabel(cell.InnerText))
            {
                continue;
            }

            var valueCell = cell.SelectSingleNode("following-sibling::td[1]");
            var text = ExtractPriceText(valueCell);
            if (!string.IsNullOrWhiteSpace(text))
            {
                yield return text;
            }
        }
    }

    private static string? ExtractPriceText(HtmlNode? valueCell)
    {
        if (valueCell is null)
        {
            return null;
        }

        var priceSpan = valueCell.SelectSingleNode(".//span[contains(concat(' ', normalize-space(@class), ' '), ' txt ')]");
        return (priceSpan ?? valueCell).InnerText.Trim();
    }

    private static bool IsCotationLabel(string value)
    {
        var normalized = NormalizeLabel(value);
        return normalized is "cotacao"
            or "cotacaoatual"
            or "ultimacotacao"
            or "ultimopreco"
            or "ultimovalor"
            or "preco"
            or "precoatual"
            or "valor"
            or "valoratual";
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
