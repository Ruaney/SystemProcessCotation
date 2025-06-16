using HtmlAgilityPack;

public class CotationService : ICotationService
{
    private readonly HttpClient _httpClient;
    public CotationService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }
    public async Task<CotationResult> GetCotationAsync(string symbol)
    {
        try
        {
            var url = $"https://www.fundamentus.com.br/detalhes.php?papel={symbol}";
            var response = await _httpClient.GetStringAsync(url);
            var doc = new HtmlDocument();
            doc.LoadHtml(response);
            var cotationNode = doc.DocumentNode.SelectSingleNode("//table[1]//tr[1]//td[@class='data destaque w3']/span[@class='txt']");

            if (cotationNode != null)
            {
                var cotationText = cotationNode.InnerText.Trim();
                if (decimal.TryParse(cotationText.Replace(",", "."), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal price))
                {
                    var result = new CotationResult
                    {
                        Symbol = symbol,
                        Price = (double)price,
                        Timestamp = DateTime.Now
                    };
                    Console.WriteLine($"Cotação {symbol}: R$ {price:F2} ({DateTime.Now:HH:mm:ss})");
                    return result;
                }

            }
            throw new Exception($"Não foi possivel extrair a cotação para {symbol}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao buscar cotação: {ex}");
        }
    }

}