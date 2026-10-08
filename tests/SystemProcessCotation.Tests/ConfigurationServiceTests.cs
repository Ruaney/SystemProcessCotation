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
        "SMTP_ENABLE_SSL",
        "SMTP_SSL",
        "MAIL_HOST",
        "MAIL_PORT",
        "MAIL_FROM",
        "MAIL_TO",
        "MAIL_PASSWORD",
        "MAIL_USERNAME",
        "MAIL_USER",
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
    public void LoadSmtpSettings_UnquotesEnvironmentValues()
    {
        var previousValues = SaveEnvironment();

        try
        {
            Environment.SetEnvironmentVariable("HOST", "\"smtp.example.com\"");
            Environment.SetEnvironmentVariable("PORT", "'587'");
            Environment.SetEnvironmentVariable("FROM", "\"alerts@example.com\"");
            Environment.SetEnvironmentVariable("TO", "'user@example.com'");
            Environment.SetEnvironmentVariable("USERNAME", "\"alerts@example.com\"");
            Environment.SetEnvironmentVariable("PASSWORD", "'secret'");
            Environment.SetEnvironmentVariable("ENABLE_SSL", "'off'");

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
    public void LoadSmtpSettings_StripsInlineCommentsFromEnvironmentValues()
    {
        var previousValues = SaveEnvironment();

        try
        {
            ClearEnvironment();
            Environment.SetEnvironmentVariable("HOST", "smtp.example.com # primary relay");
            Environment.SetEnvironmentVariable("PORT", "587 # tls port");
            Environment.SetEnvironmentVariable("FROM", "alerts@example.com # sender");
            Environment.SetEnvironmentVariable("TO", "user@example.com # recipient");
            Environment.SetEnvironmentVariable("USERNAME", "alerts@example.com # auth user");
            Environment.SetEnvironmentVariable("PASSWORD", "abc#123");
            Environment.SetEnvironmentVariable("ENABLE_SSL", "off # local smtp");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.Equal("smtp.example.com", settings.Host);
            Assert.Equal(587, settings.Port);
            Assert.Equal("alerts@example.com", settings.FromAddress);
            Assert.Equal("user@example.com", settings.ToAddress);
            Assert.Equal("alerts@example.com", settings.Username);
            Assert.Equal("abc#123", settings.Password);
            Assert.False(settings.EnableSsl);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    [Fact]
    public void LoadSmtpSettings_UsesSmtpSslAliasWhenShortFlagIsInvalid()
    {
        var previousValues = SaveEnvironment();

        try
        {
            ClearEnvironment();
            Environment.SetEnvironmentVariable("ENABLE_SSL", "maybe");
            Environment.SetEnvironmentVariable("SMTP_ENABLE_SSL", "off");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.False(settings.EnableSsl);
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
    public void LoadSmtpSettings_ReadsMailAliasesWhenShortAndSmtpNamesAreAbsent()
    {
        var previousValues = SaveEnvironment();

        try
        {
            ClearEnvironment();
            Environment.SetEnvironmentVariable("MAIL_HOST", "smtp.example.com");
            Environment.SetEnvironmentVariable("MAIL_PORT", "2525");
            Environment.SetEnvironmentVariable("MAIL_FROM", "alerts@example.com");
            Environment.SetEnvironmentVariable("MAIL_TO", "user@example.com");
            Environment.SetEnvironmentVariable("MAIL_USERNAME", "alerts@example.com");
            Environment.SetEnvironmentVariable("MAIL_PASSWORD", "secret");
            Environment.SetEnvironmentVariable("MAIL_ENABLE_SSL", "off");

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
    public void LoadSmtpSettings_UsesAliasWhenCanonicalValueCleansToBlank()
    {
        var previousValues = SaveEnvironment();

        try
        {
            ClearEnvironment();
            Environment.SetEnvironmentVariable("HOST", "' '");
            Environment.SetEnvironmentVariable("SMTP_HOST", "smtp.example.com");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.Equal("smtp.example.com", settings.Host);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    [Fact]
    public void LoadSmtpSettings_UsesSmtpPortAliasWhenShortPortIsInvalid()
    {
        var previousValues = SaveEnvironment();

        try
        {
            ClearEnvironment();
            Environment.SetEnvironmentVariable("PORT", "fast");
            Environment.SetEnvironmentVariable("SMTP_PORT", "2525");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.Equal(2525, settings.Port);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    [Fact]
    public void LoadSmtpSettings_UsesSmtpPortAliasWhenShortPortIsNonPositive()
    {
        var previousValues = SaveEnvironment();

        try
        {
            ClearEnvironment();
            Environment.SetEnvironmentVariable("PORT", "0");
            Environment.SetEnvironmentVariable("SMTP_PORT", "2525");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.Equal(2525, settings.Port);
        }
        finally
        {
            RestoreEnvironment(previousValues);
        }
    }

    [Fact]
    public void LoadSmtpSettings_UsesSmtpPortAliasWhenShortPortIsOutOfRange()
    {
        var previousValues = SaveEnvironment();

        try
        {
            ClearEnvironment();
            Environment.SetEnvironmentVariable("PORT", "70000");
            Environment.SetEnvironmentVariable("SMTP_PORT", "2525");

            var settings = new global::ConfigurationService().LoadSmtpSettings();

            Assert.Equal(2525, settings.Port);
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
    [InlineData(" off ")]
    [InlineData(" não ")]
    [InlineData(" disabled ")]
    public void LoadSmtpSettings_TrimsFalseSslAliases(string value)
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
