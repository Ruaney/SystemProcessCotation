//import in c# style

using System.Net;
using System.Net.Mail;
using MimeKit;

public class EmailService : IEmailService
{

    public void SendAlert(string to, string from, string subject, string message, SmtpSettings settings)
    {
        try
        {
            using var client = new SmtpClient(settings.Host, settings.Port)
            {
                Credentials = new NetworkCredential(settings.Username, settings.Password),
                EnableSsl = settings.EnableSsl
            };

            using var mailMessage = new MailMessage(from, to, subject, message);
            client.Send(mailMessage);
            Console.WriteLine("Email enviado");
        }
        catch (Exception ex)
        {
            throw new Exception($"Error ao enviar email: {ex}");
        }
    }
}