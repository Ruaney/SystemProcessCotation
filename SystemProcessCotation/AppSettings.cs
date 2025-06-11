public class AppSettings{
    public SmtpSettings SmtpSettings{get;set;} = new();
    public TradingSettings TradingSettings {get;set;} = new();
}
public class TradingSettings{
    public string StockSymbol {get; set;} = string.Empty;
    public double PriceToSell {get; set;}
    public double PriceToBuy {get;set; }
    public int CheckIntervalMs {get; set;}
}