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
        try
        {
            var url = $"https://www.fundamentus.com.br/detalhes.php?papel={symbol}";
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var doc = new HtmlDocument();
            doc.LoadHtml(response);
            var cotationNode = doc.DocumentNode.SelectSingleNode("//table[1]//tr[1]//td[@class='data destaque w3']/span[@class='txt']");

            if (cotationNode != null)
            {
                var cotationText = cotationNode.InnerText.Trim();
                if (decimal.TryParse(cotationText.Replace(",", "."), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal price))
                {
                    return new CotationResult
                    {
                        Symbol = symbol,
                        Price = (double)price,
                        Timestamp = DateTime.Now
                    };
                }
            }
            throw new Exception($"Não foi possivel extrair a cotação para {symbol}");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao buscar cotação: {ex}");
        }
    }
}
