using System.Globalization;

public static class CommandLineHelper
{
    public static TradingSettings ParseArguments(string[] args)
    {
        if (args.Length != 3)
        {
            throw new ArgumentException("Número incorreto de parâmetros. Esperado: 3 parâmetros");
        }

        var stockSymbol = args[0].Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(stockSymbol))
        {
            throw new ArgumentException("O código do ativo é obrigatório.");
        }

        if (!double.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var sellPrice))
        {
            throw new ArgumentException($"Preço de venda inválido: {args[1]}. Use formato decimal com ponto (ex: 22.67)");
        }

        if (!double.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var buyPrice))
        {
            throw new ArgumentException($"Preço de compra inválido: {args[2]}. Use formato decimal com ponto (ex: 22.67)");
        }

        if (sellPrice <= buyPrice)
        {
            throw new ArgumentException("O preço de venda deve ser maior que o preço de compra.");
        }

        if (sellPrice <= 0 || buyPrice <= 0)
        {
            throw new ArgumentException("Os preços devem ser maiores que zero.");
        }

        return new TradingSettings
        {
            StockSymbol = stockSymbol,
            PriceToSell = sellPrice,
            PriceToBuy = buyPrice
        };
    }
}
