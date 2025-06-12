public interface ICotationService
{
    Task<CotationResult> GetCotationAsync(string symbol);
}