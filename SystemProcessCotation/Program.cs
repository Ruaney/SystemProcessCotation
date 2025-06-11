using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Threading;
using DotNetEnv;
using System.Threading.Tasks;

public class Program
{

    public static async Task Main(string[] args)
    {
        try
        {
            args = new string [] {"PETR4", "22.67", "22.59"};
            TradingSettings tradingSettings = CommandLineHelper.ParseArguments(args);

            var configService = ConfigurationService.Instance;
            var appSettings = configService.GetAppSettings(tradingSettings);
            Console.WriteLine($"Monitorando: {appSettings.SmtpSettings.Host}");
            RunMonitoringAsync(appSettings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no monitoramento: {ex.Message}");
            await Task.Delay(3000);
        }


    }
    private static double getCotation(string cotation)
    {
        Random random = new Random();
        double currentPrice = random.NextDouble() * (23.0 - 22.0) + 22.0;
        Console.WriteLine($"Cotação atual de {cotation}: {currentPrice:F2}");
        return currentPrice;
    }

    private static void RunMonitoringAsync(AppSettings appSettings)
    {
        var settings = appSettings.TradingSettings;
        while (true)
        {

            Thread.Sleep(3000);
            var cotation = getCotation(settings.StockSymbol);
            if (cotation >= settings.PriceToSell)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Target: {settings.PriceToSell} Venda {settings.StockSymbol} a {cotation:F2}");
                Console.ResetColor();
            }
            else if (cotation <= settings.PriceToBuy)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Target: {settings.PriceToBuy} Compra {settings.StockSymbol} a {cotation:F2}");
                Console.ResetColor();
            }
        }
    }
}
