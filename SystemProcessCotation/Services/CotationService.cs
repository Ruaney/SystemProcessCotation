using System.Globalization;
using HtmlAgilityPack;

public class CotationService : ICotationService
{
    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

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

            var cotationText = ExtractCotationText(doc);
            if (cotationText is not null && TryParsePrice(cotationText, out var price))
            {
                return new CotationResult
                {
                    Symbol = normalizedSymbol,
                    Price = price,
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
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedSymbol))
        {
            throw new ArgumentException("O código do ativo é obrigatório.", nameof(symbol));
        }

        return normalizedSymbol;
    }

    private static string? ExtractCotationText(HtmlDocument doc)
    {
        var selectors = new[]
        {
            "//td[normalize-space()='Cotação']/following-sibling::td[1]//span[contains(@class,'txt')]",
            "//table[1]//tr[1]//td[contains(@class,'data') and contains(@class,'destaque') and contains(@class,'w3')]//span[contains(@class,'txt')]"
        };

        foreach (var selector in selectors)
        {
            var node = doc.DocumentNode.SelectSingleNode(selector);
            if (node is not null)
            {
                return node.InnerText.Trim();
            }
        }

        return null;
    }

    private static bool TryParsePrice(string value, out double price)
    {
        var normalizedValue = value
            .Replace("R$", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("\u00a0", string.Empty)
            .Trim();

        if (decimal.TryParse(normalizedValue, NumberStyles.Number, BrazilianCulture, out var parsedPrice) ||
            decimal.TryParse(normalizedValue, NumberStyles.Float, InvariantCulture, out parsedPrice))
        {
            price = (double)parsedPrice;
            return true;
        }

        price = 0;
        return false;
    }
}
