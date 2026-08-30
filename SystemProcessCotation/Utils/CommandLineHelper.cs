public static class CommandLineHelper
{
    public static TradingSettings ParseArguments(string[] args)
    {
        if (args.Length != 3)
        {
            throw new ArgumentException("Número incorreto de parâmetros. Esperado: 3 parâmetros");
        }

        if (!PriceParser.TryParse(args[1], out var sellPrice))
        {
            throw new ArgumentException($"Preço de venda inválido: {args[1]}. Use formato decimal com ponto ou vírgula (ex: 22.67 ou 22,67)");
        }

        if (!PriceParser.TryParse(args[2], out var buyPrice))
        {
            throw new ArgumentException($"Preço de compra inválido: {args[2]}. Use formato decimal com ponto ou vírgula (ex: 22.67 ou 22,67)");
        }

        return new TradingSettings
        {
            StockSymbol = args[0],
            PriceToSell = sellPrice,
            PriceToBuy = buyPrice
        }.NormalizeAndValidate();
    }

}
