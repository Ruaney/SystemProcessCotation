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

    [Theory]
    [InlineData("cotations.fifo", true)]
    [InlineData("COTATIONS.FIFO", true)]
    [InlineData("cotations", false)]
    public void IsFifoChannel_RecognizesTheAwsFifoSuffix(string channel, bool expected)
    {
        Assert.Equal(expected, global::SnsSqsEventBus.IsFifoChannel(channel));
    }

    [Fact]
    public async Task EnsureChannelAsync_AcceptsFifoQueueNameBeforeCallingAws()
    {
        await Assert.ThrowsAsync<NullReferenceException>(() =>
            _bus.EnsureChannelAsync("cotations.fifo", CancellationToken.None));
    }

    [Fact]
    public void CreateQueueRequest_ConfiguresFifoQueues()
    {
        var request = global::SnsSqsEventBus.CreateQueueRequest("cotations.fifo");

        Assert.Equal("cotations.fifo", request.QueueName);
        Assert.Equal("true", request.Attributes["FifoQueue"]);
    }

    [Fact]
    public void CreateQueueRequest_KeepsStandardQueuesUnchanged()
    {
        var request = global::SnsSqsEventBus.CreateQueueRequest("cotations");

        Assert.Empty(request.Attributes);
    }

    [Fact]
    public void CreateTopicRequest_ConfiguresContentDeduplicationForFifoTopics()
    {
        var request = global::SnsSqsEventBus.CreateTopicRequest("cotations.fifo");

        Assert.Equal("cotations.fifo", request.Name);
        Assert.Equal("true", request.Attributes["FifoTopic"]);
        Assert.Equal("true", request.Attributes["ContentBasedDeduplication"]);
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

    [Fact]
    public async Task TryHandleSqsMessageAsync_DropsInvalidDomainPayloadWithoutCallingHandler()
    {
        var handlerCalled = false;

        var shouldDelete = await global::SnsSqsEventBus.TryHandleSqsMessageAsync<CotationResult>(
            new Message { Body = """{"symbol":"PETR4","price":0}""" },
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

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("receipt-1", true)]
    public void CanDeleteSqsMessage_RequiresReceiptHandle(string? receiptHandle, bool expected)
    {
        var canDelete = global::SnsSqsEventBus.CanDeleteSqsMessage(new Message
        {
            ReceiptHandle = receiptHandle
        });

        Assert.Equal(expected, canDelete);
    }
}
