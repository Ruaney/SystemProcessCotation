using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Threading;
using DotNetEnv;

public class Program
{
    
    public static void Main(string[] args)
    {
        DotNetEnv.Env.Load("../../../.env");
        string from = Environment.GetEnvironmentVariable("FROM") ?? throw new InvalidOperationException("FROM environment variable not set");
        string to = Environment.GetEnvironmentVariable("TO") ?? throw new InvalidOperationException("TO environment variable not set");
        string smtpServer = Environment.GetEnvironmentVariable("SMTP") ?? throw new InvalidOperationException("SMTP environment variable not set");
        string smtpPort = Environment.GetEnvironmentVariable("SMTP_PORT") ?? throw new InvalidOperationException("SMTP_PORT environment variable not set");
        string password = Environment.GetEnvironmentVariable("PASSWORD") ?? throw new InvalidOperationException("PASSWORD environment variable not set");
        string ativo = "PETR4";
        double priceToSell = 22.67;
        double priceToBuy = 22.40;
    
        Console.WriteLine($"From: {from}, To: {to}, SMTP: {smtpServer}, Port: {smtpPort}");

        void sendEmailSMTP(string message)
        {
            try
            {
                var client = new System.Net.Mail.SmtpClient(smtpServer, int.Parse(smtpPort))
                {
                    Credentials = new NetworkCredential(from, password),
                    EnableSsl = true
                };
              
                client.Send(from, to, "System Cotation Alert", message);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        double getCotation(string cotation)
        {
            Random random = new Random();
            double currentPrice = random.NextDouble() * (23.0 - 22.0) + 22.0;
            Console.WriteLine($"Cotação atual de {cotation}: {currentPrice:F2}");
            return currentPrice;
        }

        while (true)
        {
            Thread.Sleep(2000);
            var cotation = getCotation(ativo);
            if (cotation >= priceToSell)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Target: {priceToSell} Venda {ativo} a {cotation:F2}");
                Console.ResetColor();
                sendEmailSMTP($"Target: {priceToSell} Venda {ativo} na cotação atual: {cotation:F2}");
            }
            else if (cotation <= priceToBuy)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Target: {priceToBuy} Compra {ativo} a {cotation:F2}");
                Console.ResetColor();
                sendEmailSMTP($"Target: {priceToBuy} Compra {ativo} na cotação atual: {cotation:F2}");
            }
        }
    }
}
