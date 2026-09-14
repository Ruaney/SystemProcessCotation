namespace SystemProcessCotation.Tests;

public class EmailServiceTests
{
    [Fact]
    public async Task SendAlertAsync_ThrowsWhenCancellationAlreadyRequested()
    {
        var service = new global::EmailService();
        var settings = new global::SmtpSettings
        {
            Host = "smtp.example.com",
            Port = 587,
            Username = "alerts@example.com",
            Password = "secret"
        };
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            service.SendAlertAsync(
                "user@example.com",
                "alerts@example.com",
                "Subject",
                "Message",
                settings,
                cts.Token));
    }

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
