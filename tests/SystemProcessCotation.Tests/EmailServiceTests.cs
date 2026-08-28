namespace SystemProcessCotation.Tests;

public class EmailServiceTests
{
    [Fact]
    public async Task SendAlertAsync_WrapsInvalidMessageAddressWithOriginalException()
    {
        var service = new global::EmailService();
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            Username = "alerts@example.com",
            Password = "secret"
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SendAlertAsync(
                "user@example.com",
                string.Empty,
                "Subject",
                "Message",
                settings));

        Assert.Equal("Erro ao enviar email.", exception.Message);
        Assert.NotNull(exception.InnerException);
    }
}
