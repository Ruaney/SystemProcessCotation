using System.IO;
public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool UsesSsl { get; set; }
    public string FromAddress { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    //read from a file config
    public SmtpSettings()
    {
        FileInfo file = new FileInfo("config.txt");
        if (file.Exists)
        {
            Console.WriteLine(file.);
        }
    }
}