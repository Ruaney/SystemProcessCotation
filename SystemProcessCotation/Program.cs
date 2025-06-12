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
using SystemProcessCotation;

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
            var cotationService = new CotationService();
            RunMonitoringAsync(appSettings, cotationService);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no monitoramento: {ex.Message}");
            await Task.Delay(3000);
        }


    }
 
    private static async Task RunMonitoringAsync(AppSettings appSettings, CotationService cotationService)
    {
        var settings = appSettings.TradingSettings;
        while (true)
        {

            Thread.Sleep(3000);
            var cotation = await cotationService.GetCotationAsync(settings.StockSymbol);
            if (cotation.Price >= settings.PriceToSell)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Target: {settings.PriceToSell} Venda {settings.StockSymbol} a {cotation.Price:F2}");
                Console.ResetColor();
            }
            else if (cotation.Price <= settings.PriceToBuy)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Target: {settings.PriceToBuy} Compra {settings.StockSymbol} a {cotation.Price:F2}");
                Console.ResetColor();
            }
        }
    }
}
