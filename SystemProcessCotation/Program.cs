using System;
using System.Net;
using System.Net.Mail;

public class Program
{
    public static void Main(string[] args)
    {
        string ativo = "PETR4";
        double priceToSell = 22.67;
        double priceToBuy = 22.59;

        void sendEmailSMTP() {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("ruaneymainarth@gmail.com","123"),
                 EnableSsl = true,
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress("ruan.ramos@hotmail.com"),
                Subject = "Alerta de Cotação",
                Body = "A cotação do ativo atingiu o valor desejado.",
                IsBodyHtml = false,
            };
            mailMessage.To.Add("ruan.ramos@hotmail.com");
            smtpClient.Send(mailMessage);
        }

        double getCotation(string cotation) {
            // Simulate getting the current price of the stock
            Random random = new Random();
            double currentPrice = random.NextDouble() * (23.0 - 22.0) + 22.0; // Random price between 22.0 and 23.0
            Console.WriteLine($"Cotação atual de {cotation}: {currentPrice}");
            return 0.0;
        }

        while (true)
        {
            Thread.Sleep(2000);
            var cotation = getCotation(ativo);
            if (priceToSell > cotation)
            {
                Console.WriteLine($"Venda {ativo} a {priceToSell}");
                sendEmailSMTP();
            }
            else if(priceToBuy < cotation)
            {
                Console.WriteLine($"Compra {ativo} a {priceToBuy}");
                sendEmailSMTP();
            }
        }
    }
}
