using HtmlAgilityPack;

public class CotationService : ICotationService
{
    private readonly HttpClient _httpClient;

    public CotationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CotationResult> GetCotationAsync(string symbol, CancellationToken cancellationToken = default)
    {
        var normalizedSymbol = NormalizeSymbol(symbol);

        try
        {
            var url = $"https://www.fundamentus.com.br/detalhes.php?papel={Uri.EscapeDataString(normalizedSymbol)}";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
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

    private static string NormalizeSymbol(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("O código do ativo é obrigatório.", nameof(symbol));
        }

        return symbol.Trim().ToUpperInvariant();
    }

    private static double? ExtractCotationPrice(HtmlDocument doc)
    {
        var selectors = new[]
        {
            "//td[normalize-space()='Cotação']/following-sibling::td[1]",
            "//table[1]//td[contains(@class,'data') and contains(@class,'destaque') and contains(@class,'w3')]//span[contains(@class,'txt')]"
        };

        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode(selector);
            var text = node?.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(text) && PriceParser.TryParse(text, out var price) && price > 0)
            {
                return price;
            }
        }

        return null;
    }
}
