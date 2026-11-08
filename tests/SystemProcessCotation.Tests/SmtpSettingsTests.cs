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

    [Fact]
    public void IsConfigured_AcceptsAnIpv6SmtpHost()
    {
        var settings = new global::SmtpSettings
        {
            Host = "2001:db8::1",
            Port = 587,
            FromAddress = "alerts@example.com",
            ToAddress = "user@example.com",
            Username = "alerts@example.com",
            Password = "secret"
        };

        Assert.True(settings.IsConfigured);
    }

    [Fact]
    public void IsConfigured_AcceptsFullyQualifiedDnsHost()
    {
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com.",
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

    [Theory]
    [InlineData("")]
    [InlineData("smtp example.com")]
    [InlineData("http://smtp.example.com")]
    public void IsConfigured_ReturnsFalseWhenHostIsInvalid(string host)
    {
        var settings = new global::SmtpSettings
        {
            Host = host,
            Port = 587,
            FromAddress = "alerts@example.com",
            ToAddress = "user@example.com",
            Username = "alerts@example.com",
            Password = "secret"
        };

        Assert.False(settings.IsConfigured);
    }

    [Fact]
    public void IsConfigured_ReturnsFalseWhenHostExceedsDnsLimit()
    {
        var settings = new global::SmtpSettings
        {
            Host = $"{new string('a', 63)}.{new string('b', 63)}.{new string('c', 63)}.{new string('d', 63)}",
            Port = 587,
            FromAddress = "alerts@example.com",
            ToAddress = "user@example.com",
            Username = "alerts@example.com",
            Password = "secret"
        };

        Assert.False(settings.IsConfigured);
    }

    [Fact]
    public void IsConfigured_ReturnsFalseWhenDnsLabelExceedsLimit()
    {
        var settings = new global::SmtpSettings
        {
            Host = $"{new string('a', 64)}.example.com",
            Port = 587,
            FromAddress = "alerts@example.com",
            ToAddress = "user@example.com",
            Username = "alerts@example.com",
            Password = "secret"
        };

        Assert.False(settings.IsConfigured);
    }

    [Theory]
    [InlineData("-smtp.example.com")]
    [InlineData("smtp-.example.com")]
    public void IsConfigured_ReturnsFalseWhenDnsLabelHasInvalidHyphenPlacement(string host)
    {
        var settings = new global::SmtpSettings
        {
            Host = host,
            Port = 587,
            FromAddress = "alerts@example.com",
            ToAddress = "user@example.com",
            Username = "alerts@example.com",
            Password = "secret"
        };

        Assert.False(settings.IsConfigured);
    }

    [Fact]
    public void IsConfigured_ReturnsFalseWhenHostContainsInvisibleFormatCharacters()
    {
        var settings = new global::SmtpSettings
        {
            Host = "smtp\u200B.example.com",
            Port = 587,
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

    [Fact]
    public void IsConfigured_ReturnsFalseWhenUsernameContainsInvisibleFormatCharacters()
    {
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            FromAddress = "alerts@example.com",
            ToAddress = "user@example.com",
            Username = "alerts\u200B@example.com",
            Password = "secret"
        };

        Assert.False(settings.IsConfigured);
    }

    [Fact]
    public void IsConfigured_ReturnsFalseWhenPasswordContainsInvisibleFormatCharacters()
    {
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            FromAddress = "alerts@example.com",
            ToAddress = "user@example.com",
            Username = "alerts@example.com",
            Password = "sec\u200Bret"
        };

        Assert.False(settings.IsConfigured);
    }

    [Fact]
    public void IsConfigured_ReturnsFalseWhenPasswordContainsControlCharacters()
    {
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            FromAddress = "alerts@example.com",
            ToAddress = "user@example.com",
            Username = "alerts@example.com",
            Password = "secret\r\nvalue"
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

    [Fact]
    public void IsConfigured_ReturnsFalseWhenAddressContainsControlCharacters()
    {
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            FromAddress = "alerts@example.com\r\nBcc: other@example.com",
            ToAddress = "user@example.com",
            Username = "alerts@example.com",
            Password = "secret"
        };

        Assert.False(settings.IsConfigured);
    }

    [Fact]
    public void IsConfigured_ReturnsFalseWhenAddressContainsInvisibleFormatCharacters()
    {
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            FromAddress = "alerts@example.com",
            ToAddress = "user\u200B@example.com",
            Username = "alerts@example.com",
            Password = "secret"
        };

        Assert.False(settings.IsConfigured);
    }

    [Theory]
    [InlineData("alerts@invalid_domain.example")]
    [InlineData("alerts@a..example.com")]
    public void IsConfigured_ReturnsFalseWhenEmailDomainIsInvalid(string from)
    {
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            FromAddress = from,
            ToAddress = "user@example.com",
            Username = "alerts@example.com",
            Password = "secret"
        };

        Assert.False(settings.IsConfigured);
    }
}
