namespace SystemProcessCotation.Tests;

public class ConfigurationServiceTests
{
    private static readonly string[] SmtpVariables =
    [
        "HOST",
        "PORT",
        "FROM",
        "TO",
        "PASSWORD",
        "USERNAME",
        "ENABLE_SSL",
        "SMTP_HOST",
        "SMTP_PORT",
        "SMTP_FROM",
        "SMTP_TO",
        "SMTP_PASSWORD",
        "SMTP_USERNAME",
        "SMTP_USER",
        "SMTP_USER_NAME",
        "SMTP_ENABLE_SSL",
        "SMTP_SSL",
        "SMTP_USE_SSL",
        "SMTP_SSL_ENABLED",
        "EMAIL_HOST",
        "EMAIL_PORT",
        "EMAIL_FROM",
        "EMAIL_TO",
        "EMAIL_USERNAME",
        "EMAIL_USER",
        "EMAIL_PASSWORD",
        "EMAIL_ENABLE_SSL",
        "SMTP_FROM_ADDRESS",
        "SMTP_TO_ADDRESS",
        "MAIL_HOST",
        "MAIL_PORT",
        "MAIL_FROM",
        "MAIL_TO",
        "MAIL_USERNAME",
        "MAIL_USER",
        "MAIL_PASSWORD",
        "MAIL_ENABLE_SSL",
        "MAIL_SSL"
    ];

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
            Environment.SetEnvironmentVariable("ENABLE_SSL", " false ");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.Equal("smtp.example.com", settings.Host);
            Assert.Equal(587, settings.Port);
            Assert.Equal("alerts@example.com", settings.FromAddress);
            Assert.Equal("user@example.com", settings.ToAddress);
            Assert.Equal("alerts@example.com", settings.Username);
            Assert.Equal("secret", settings.Password);
            Assert.False(settings.EnableSsl);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    [Fact]
    public void LoadSmtpSettings_DefaultsEnableSslToTrueWhenFlagIsInvalid()
    {
        var previousValues = SaveEnvironment();

        try
        {
            Environment.SetEnvironmentVariable("ENABLE_SSL", "maybe");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.True(settings.EnableSsl);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    [Fact]
    public void LoadSmtpSettings_ReadsSmtpAliasesWhenCanonicalNamesAreAbsent()
    {
        var previousValues = SaveEnvironment();

        try
        {
            ClearEnvironment();
            Environment.SetEnvironmentVariable("SMTP_HOST", "smtp.example.com");
            Environment.SetEnvironmentVariable("SMTP_PORT", "2525");
            Environment.SetEnvironmentVariable("SMTP_FROM", "alerts@example.com");
            Environment.SetEnvironmentVariable("SMTP_TO", "user@example.com");
            Environment.SetEnvironmentVariable("SMTP_USERNAME", "alerts@example.com");
            Environment.SetEnvironmentVariable("SMTP_PASSWORD", "secret");
            Environment.SetEnvironmentVariable("SMTP_ENABLE_SSL", "off");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.Equal("smtp.example.com", settings.Host);
            Assert.Equal(2525, settings.Port);
            Assert.Equal("alerts@example.com", settings.FromAddress);
            Assert.Equal("user@example.com", settings.ToAddress);
            Assert.Equal("alerts@example.com", settings.Username);
            Assert.Equal("secret", settings.Password);
            Assert.False(settings.EnableSsl);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    [Fact]
    public void LoadSmtpSettings_ReadsEmailAliasesWhenSmtpNamesAreAbsent()
    {
        var previousValues = SaveEnvironment();

        try
        {
            ClearEnvironment();
            Environment.SetEnvironmentVariable("EMAIL_HOST", "smtp.example.com");
            Environment.SetEnvironmentVariable("EMAIL_PORT", "2525");
            Environment.SetEnvironmentVariable("EMAIL_FROM", "alerts@example.com");
            Environment.SetEnvironmentVariable("EMAIL_TO", "user@example.com");
            Environment.SetEnvironmentVariable("EMAIL_USERNAME", "alerts@example.com");
            Environment.SetEnvironmentVariable("EMAIL_PASSWORD", "secret");
            Environment.SetEnvironmentVariable("EMAIL_ENABLE_SSL", "0");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.Equal("smtp.example.com", settings.Host);
            Assert.Equal(2525, settings.Port);
            Assert.Equal("alerts@example.com", settings.FromAddress);
            Assert.Equal("user@example.com", settings.ToAddress);
            Assert.Equal("alerts@example.com", settings.Username);
            Assert.Equal("secret", settings.Password);
            Assert.False(settings.EnableSsl);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    [Fact]
    public void LoadSmtpSettings_ReadsMailAliasesWhenOtherNamesAreAbsent()
    {
        var previousValues = SaveEnvironment();

        try
        {
            ClearEnvironment();
            Environment.SetEnvironmentVariable("MAIL_HOST", "smtp.example.com");
            Environment.SetEnvironmentVariable("MAIL_PORT", "2525");
            Environment.SetEnvironmentVariable("MAIL_FROM", "alerts@example.com");
            Environment.SetEnvironmentVariable("MAIL_TO", "user@example.com");
            Environment.SetEnvironmentVariable("MAIL_USER", "alerts@example.com");
            Environment.SetEnvironmentVariable("MAIL_PASSWORD", "secret");
            Environment.SetEnvironmentVariable("MAIL_SSL", "false");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.Equal("smtp.example.com", settings.Host);
            Assert.Equal(2525, settings.Port);
            Assert.Equal("alerts@example.com", settings.FromAddress);
            Assert.Equal("user@example.com", settings.ToAddress);
            Assert.Equal("alerts@example.com", settings.Username);
            Assert.Equal("secret", settings.Password);
            Assert.False(settings.EnableSsl);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    [Fact]
    public void LoadSmtpSettings_PrefersCanonicalNamesOverAliases()
    {
        var previousValues = SaveEnvironment();

        try
        {
            ClearEnvironment();
            Environment.SetEnvironmentVariable("HOST", "smtp.primary.example.com");
            Environment.SetEnvironmentVariable("SMTP_HOST", "smtp.alias.example.com");
            Environment.SetEnvironmentVariable("USERNAME", "primary@example.com");
            Environment.SetEnvironmentVariable("SMTP_USER_NAME", "alias@example.com");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.Equal("smtp.primary.example.com", settings.Host);
            Assert.Equal("primary@example.com", settings.Username);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    [Theory]
    [InlineData("não")]
    [InlineData("off")]
    [InlineData("disabled")]
    public void LoadSmtpSettings_DisablesSslForFalseAliases(string value)
    {
        var previousValues = SaveEnvironment();

        try
        {
            Environment.SetEnvironmentVariable("ENABLE_SSL", value);

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.False(settings.EnableSsl);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    [Theory]
    [InlineData("on")]
    [InlineData("enabled")]
    [InlineData("sim")]
    public void LoadSmtpSettings_EnablesSslForTrueAliases(string value)
    {
        var previousValues = SaveEnvironment();

        try
        {
            Environment.SetEnvironmentVariable("ENABLE_SSL", value);

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.True(settings.EnableSsl);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    private static Dictionary<string, string?> SaveEnvironment() =>
        SmtpVariables.ToDictionary(name => name, Environment.GetEnvironmentVariable);

    private static void ClearEnvironment()
    {
        foreach (var name in SmtpVariables)
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    private static void RestoreEnvironment(Dictionary<string, string?> values)
    {
        foreach (var (name, value) in values)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}
