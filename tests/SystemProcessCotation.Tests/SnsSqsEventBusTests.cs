using Amazon.SQS.Model;
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
    [InlineData("cotações")]
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

    [Fact]
    public async Task TryHandleSqsMessageAsync_DropsBlankBodyWithoutCallingHandler()
    {
        var handlerCalled = false;

        var shouldDelete = await global::SnsSqsEventBus.TryHandleSqsMessageAsync<CotationResult>(
            new Message { Body = "   " },
            (_, _) =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<global::SnsSqsEventBus>.Instance,
            "cotations",
            CancellationToken.None);

        Assert.True(shouldDelete);
        Assert.False(handlerCalled);
    }

    [Fact]
    public async Task TryHandleSqsMessageAsync_DispatchesValidPayload()
    {
        CotationResult? handled = null;

        var shouldDelete = await global::SnsSqsEventBus.TryHandleSqsMessageAsync<CotationResult>(
            new Message { Body = """{"symbol":"PETR4","price":31.42}""" },
            (message, _) =>
            {
                handled = message;
                return Task.CompletedTask;
            },
            NullLogger<global::SnsSqsEventBus>.Instance,
            "cotations",
            CancellationToken.None);

        Assert.True(shouldDelete);
        Assert.NotNull(handled);
        Assert.Equal("PETR4", handled.Symbol);
        Assert.Equal(31.42, handled.Price);
    }

    [Fact]
    public async Task TryHandleSqsMessageAsync_DropsMalformedPayloadWithoutCallingHandler()
    {
        var handlerCalled = false;

        var shouldDelete = await global::SnsSqsEventBus.TryHandleSqsMessageAsync<CotationResult>(
            new Message { Body = "{" },
            (_, _) =>
            {
                handlerCalled = true;
                return Task.CompletedTask;
            },
            NullLogger<global::SnsSqsEventBus>.Instance,
            "cotations",
            CancellationToken.None);

        Assert.True(shouldDelete);
        Assert.False(handlerCalled);
    }
}
