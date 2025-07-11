﻿
public class Program
{

    public static async Task Main(string[] args)
    {
        try
        {
            TradingSettings tradingSettings = CommandLineHelper.ParseArguments(args);

            var configService = ConfigurationService.Instance;
            var appSettings = configService.GetAppSettings(tradingSettings);
            await RunMonitoringAsync(appSettings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no aplicação: {ex.Message}");
            await Task.Delay(3000);
        }
    }

    private static async Task RunMonitoringAsync(AppSettings appSettings)
    {
        var settings = appSettings.TradingSettings;
        var tradingService = new TradingService();
        var cotationService = new CotationService();
        var emailService = new EmailService();
        double lastPrice = 0.0;
        var alertCount = 0;
        while (true)
        {
            try
            {
                Console.WriteLine("Sistema de cotação");
                Console.WriteLine("--------------------------------");
                Console.WriteLine($"Ativo: {settings.StockSymbol}");
                Console.WriteLine($"Venda quando >= R$ {settings.PriceToSell:F2}");
                Console.WriteLine($"Compra quando <= R$ {settings.PriceToBuy:F2}");
                Console.WriteLine($"Enviar alerta para: {appSettings.SmtpSettings.ToAddress}");
                Console.WriteLine("--------------------------------");

                var cotation = await cotationService.GetCotationAsync(settings.StockSymbol);

                var alert = await tradingService.AnalyzeCotationAsync(cotation, settings);
                
                if (alert != null && lastPrice != cotation.Price)
                {
                    emailService.SendAlert(appSettings.SmtpSettings.ToAddress, appSettings.SmtpSettings.FromAddress, alert.GetSubject(), alert.GetMessage(), appSettings.SmtpSettings);
                    alertCount++;
                    lastPrice = cotation.Price;
                }
                Console.WriteLine("Total de alertas enviados: " + alertCount);
                Thread.Sleep(3000);
                Console.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no monitoramento: {ex.Message}");
                break;
            }
        }
    }
}
