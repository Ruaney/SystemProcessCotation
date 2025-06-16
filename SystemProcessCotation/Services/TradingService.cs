using System.ComponentModel;

public class TradingService : ITradingService
{
    private readonly HashSet<string> _recentAlerts = new();
    private readonly TimeSpan _alertCooldown = TimeSpan.FromMinutes(1);
    private readonly Dictionary<string, DateTime> _lastAlertTimes = new();
    public Task<TradingAlert?> AnalyzeCotationAsync(CotationResult cotation, TradingSettings settings)
    {
        if (!cotation.IsValid)
        {
            return Task.FromResult<TradingAlert?>(null);
        }
        TradingAlert? alert = null;
        if (cotation.Price < settings.PriceToBuy && cotation.Price < settings.PriceToSell)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Atenção: Informe um preço de compra e venda entre o preço atual do ativo para maior consistência dos alertas.");
            Console.ResetColor();
        }
        if (cotation.Price >= settings.PriceToSell)
        {
            alert = new TradingAlert
            {
                Type = AlertType.Sell,
                Symbol = cotation.Symbol,
                CurrentPrice = cotation.Price,
                TargetPrice = settings.PriceToSell
            };
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Venda: {cotation.Symbol} R$ {cotation.Price:F2} (target: R$ {settings.PriceToSell:F2})");
            Console.ResetColor();
        }
        else if (cotation.Price <= settings.PriceToBuy)
        {
            alert = new TradingAlert
            {
                Type = AlertType.Buy,
                Symbol = cotation.Symbol,
                CurrentPrice = cotation.Price,
                TargetPrice = settings.PriceToBuy
            };

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"COMPRA: {cotation.Symbol} R$ {cotation.Price:F2} (target: R$ {settings.PriceToBuy:F2})");
            Console.ResetColor();
        }

        if (alert != null && ShouldSendAlert(alert))
        {
            RecordAlert(alert);
            return Task.FromResult(alert);
        }
        return Task.FromResult<TradingAlert?>(null);
    }

    private bool ShouldSendAlert(TradingAlert alert)
    {
        var alertKey = $"{alert.Symbol}_{alert.Type}";
        if (_lastAlertTimes.TryGetValue(alertKey, out var lastTime))
        {
            return DateTime.Now - lastTime >= _alertCooldown;
        }
        return true;
    }


    private void RecordAlert(TradingAlert alert)
    {
        var alertKey = $"{alert.Symbol}_{alert.Type}";
        _lastAlertTimes[alertKey] = DateTime.Now;
    }
}