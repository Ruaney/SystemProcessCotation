using System.Globalization;

public static class CommandLineHelper
{
    public static TradingSettings ParseArguments(string[] args)
    {
        if (args.Length != 3)
        {
            throw new ArgumentException("Número incorreto de parâmetros. Esperado: 3 parâmetros");
        }

        if (!double.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var sellPrice))
        {
            throw new ArgumentException($"Preço de venda inválido: {args[1]}. Use formato decimal com ponto (ex: 22.67)");
        }

        if (!double.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var buyPrice))
        {
            throw new ArgumentException($"Preço de compra inválido: {args[2]}. Use formato decimal com ponto (ex: 22.67)");
        }

        return new TradingSettings
        {
            StockSymbol = args[0],
            PriceToSell = sellPrice,
            PriceToBuy = buyPrice
        }.NormalizeAndValidate();
    }
}
