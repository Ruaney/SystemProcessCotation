namespace SystemProcessCotation.Tests;

public class SmtpSettingsTests
{
    [Fact]
    public void IsConfigured_ReturnsTrueWhenRequiredFieldsArePresent()
    {
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            FromAddress = "alerts@example.com",
            ToAddress = "user@example.com",
            Username = "alerts@example.com",
            Password = "secret"
        };

        Assert.True(settings.IsConfigured);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    public void IsConfigured_ReturnsFalseWhenPortIsInvalid(int port)
    {
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com",
            Port = port,
            FromAddress = "alerts@example.com",
            ToAddress = "user@example.com",
            Username = "alerts@example.com",
            Password = "secret"
        };

        Assert.False(settings.IsConfigured);
    }

    [Fact]
    public void IsConfigured_ReturnsFalseWhenCredentialsAreMissing()
    {
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            FromAddress = "alerts@example.com",
            ToAddress = "user@example.com"
        };

        Assert.False(settings.IsConfigured);
    }

    [Theory]
    [InlineData("alerts.example.com", "user@example.com")]
    [InlineData("alerts@example.com", "user.example.com")]
    public void IsConfigured_ReturnsFalseWhenEmailAddressIsInvalid(string from, string to)
    {
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            FromAddress = from,
            ToAddress = to,
            Username = "alerts@example.com",
            Password = "secret"
        };

        Assert.False(settings.IsConfigured);
    }
}
