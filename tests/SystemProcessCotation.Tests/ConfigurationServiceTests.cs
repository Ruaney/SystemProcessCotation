namespace SystemProcessCotation.Tests;

public class ConfigurationServiceTests
{
    private static readonly string[] SmtpVariables = ["HOST", "PORT", "FROM", "TO", "PASSWORD", "USERNAME"];

    [Fact]
    public void LoadSmtpSettings_TrimsEnvironmentValues()
    {
        var previousValues = SaveEnvironment();

        try
        {
            Environment.SetEnvironmentVariable("HOST", " smtp.example.com ");
            Environment.SetEnvironmentVariable("PORT", " 587 ");
            Environment.SetEnvironmentVariable("FROM", " alerts@example.com ");
            Environment.SetEnvironmentVariable("TO", " user@example.com ");
            Environment.SetEnvironmentVariable("USERNAME", " alerts@example.com ");
            Environment.SetEnvironmentVariable("PASSWORD", " secret ");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.Equal("smtp.example.com", settings.Host);
            Assert.Equal(587, settings.Port);
            Assert.Equal("alerts@example.com", settings.FromAddress);
            Assert.Equal("user@example.com", settings.ToAddress);
            Assert.Equal("alerts@example.com", settings.Username);
            Assert.Equal("secret", settings.Password);
            Assert.True(settings.EnableSsl);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    private static Dictionary<string, string?> SaveEnvironment() =>
        SmtpVariables.ToDictionary(name => name, Environment.GetEnvironmentVariable);

    private static void RestoreEnvironment(Dictionary<string, string?> values)
    {
        foreach (var (name, value) in values)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}
