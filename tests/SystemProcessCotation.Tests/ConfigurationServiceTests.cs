namespace SystemProcessCotation.Tests;

public class ConfigurationServiceTests : IDisposable
{
    private static readonly string[] EnvironmentNames =
    [
        "ENABLE_SSL",
        "HOST",
        "PASSWORD",
        "PORT",
        "TO",
        "FROM",
        "USERNAME"
    ];

    public void Dispose()
    {
        foreach (var name in EnvironmentNames)
        {
            Environment.SetEnvironmentVariable(name, null);
        }
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("not-a-bool", true)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("yes", true)]
    [InlineData("no", false)]
    [InlineData("on", true)]
    [InlineData("off", false)]
    public void LoadSmtpSettings_ParsesSslEnvironmentAliases(string? sslValue, bool expected)
    {
        Environment.SetEnvironmentVariable("ENABLE_SSL", sslValue);

        var settings = new global::ConfigurationService().LoadSmtpSettings();

        Assert.Equal(expected, settings.EnableSsl);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("65536")]
    [InlineData("not-a-port")]
    public void LoadSmtpSettings_NormalizesInvalidPortsToZero(string portValue)
    {
        Environment.SetEnvironmentVariable("PORT", portValue);

        var settings = new global::ConfigurationService().LoadSmtpSettings();

        Assert.Equal(0, settings.Port);
    }
}
