
public class Program
{

    public static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            Console.WriteLine("\nEncerrando o monitoramento...");
        };

        try
        {
            TradingSettings tradingSettings = CommandLineHelper.ParseArguments(args);

            var configService = ConfigurationService.Instance;
            var appSettings = configService.GetAppSettings(tradingSettings);
            await RunMonitoringAsync(appSettings, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Monitoramento encerrado.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no aplicação: {ex.Message}");
            await Task.Delay(3000);
        }
    }

    private static async Task RunMonitoringAsync(AppSettings appSettings, CancellationToken cancellationToken)
    {
        var settings = appSettings.TradingSettings;
        var tradingService = new TradingService();
        var cotationService = new CotationService();
        var emailService = new EmailService();
        var checkInterval = settings.CheckIntervalMs > 0 ? settings.CheckIntervalMs : 3000;
        double lastPrice = 0.0;
        var alertCount = 0;
        while (!cancellationToken.IsCancellationRequested)
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

                var cotation = await cotationService.GetCotationAsync(settings.StockSymbol, cancellationToken);

                var alert = await tradingService.AnalyzeCotationAsync(cotation, settings, cancellationToken);

                if (alert != null && lastPrice != cotation.Price)
                {
                    await emailService.SendAlertAsync(appSettings.SmtpSettings.ToAddress, appSettings.SmtpSettings.FromAddress, alert.GetSubject(), alert.GetMessage(), appSettings.SmtpSettings, cancellationToken);
                    alertCount++;
                    lastPrice = cotation.Price;
                }
                Console.WriteLine("Total de alertas enviados: " + alertCount);
                await Task.Delay(checkInterval, cancellationToken);
                Console.Clear();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no monitoramento: {ex.Message}");
                break;
            }
        }
    }
}
