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
using System.Xml.Schema;

public class Program
{

    public static async Task Main(string[] args)
    {
        try
        {
            args = new string[] { "PETR4", "22.67", "22.59" };
            TradingSettings tradingSettings = CommandLineHelper.ParseArguments(args);

            var configService = ConfigurationService.Instance;
            var appSettings = configService.GetAppSettings(tradingSettings);
            var cotationService = new CotationService();
            RunMonitoringAsync(appSettings, cotationService);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no aplicação: {ex.Message}");
            await Task.Delay(3000);
        }


    }

    private static async Task RunMonitoringAsync(AppSettings appSettings, CotationService cotationService)
    {
        var settings = appSettings.TradingSettings;
        var tradingService = new TradingService();


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
                Console.WriteLine($"Alerta para: {appSettings.SmtpSettings.ToAddress}");
                Console.WriteLine("--------------------------------");

                var cotation = await cotationService.GetCotationAsync(settings.StockSymbol);
                var alert = await tradingService.AnalyzeCotationAsync(cotation, settings);

                if (alert != null)
                {
                    Console.WriteLine("Alerta enviado! ");
                    alertCount++;
                }
                Console.WriteLine("Total de alertas: " + alertCount);
                Thread.Sleep(3000);
                Console.Clear();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no monitoramento: {ex.Message}");
            }
        }
    }
}
