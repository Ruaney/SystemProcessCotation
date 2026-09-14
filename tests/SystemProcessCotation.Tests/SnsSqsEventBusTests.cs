using Microsoft.Extensions.Logging.Abstractions;

namespace SystemProcessCotation.Tests;

public class SnsSqsEventBusTests
{
    private readonly global::SnsSqsEventBus _bus = new(null!, null!, NullLogger<global::SnsSqsEventBus>.Instance);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task PublishAsync_RejectsBlankChannelBeforeCallingAws(string channel)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _bus.PublishAsync(channel, new { Value = 1 }));

        Assert.Contains("canal", exception.Message);
    }

    [Theory]
    [InlineData("cotations/primary")]
    [InlineData("cotations.primary")]
    public async Task EnsureChannelAsync_RejectsInvalidAwsChannelNameBeforeCallingAws(string channel)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _bus.EnsureChannelAsync(channel, CancellationToken.None));

        Assert.Contains("letras", exception.Message);
    }

    [Fact]
    public async Task EnsureChannelAsync_RejectsChannelNamesLongerThanSqsAllows()
    {
        var channel = new string('a', 81);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _bus.EnsureChannelAsync(channel, CancellationToken.None));

        Assert.Contains("80", exception.Message);
    }

    [Fact]
    public async Task SubscribeAsync_RejectsMissingHandlerBeforeCallingAws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _bus.SubscribeAsync<CotationResult>("cotations", null!));
    }
}
