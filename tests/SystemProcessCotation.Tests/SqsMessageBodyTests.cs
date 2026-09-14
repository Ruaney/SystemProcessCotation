namespace SystemProcessCotation.Tests;

public class SqsMessageBodyTests
{
    [Fact]
    public void ExtractPayload_ReturnsRawBodyWhenMessageIsNotSnsEnvelope()
    {
        const string body = """{"symbol":"PETR4","price":31.42}""";

        var payload = global::SqsMessageBody.ExtractPayload(body);

        Assert.Equal(body, payload);
    }

    [Fact]
    public void ExtractPayload_UnwrapsSnsNotificationMessage()
    {
        const string body = """
            {
              "Type": "Notification",
              "Message": "{\"symbol\":\"PETR4\",\"price\":31.42}"
            }
            """;

        var payload = global::SqsMessageBody.ExtractPayload(body);

        Assert.Equal("""{"symbol":"PETR4","price":31.42}""", payload);
    }

    [Fact]
    public void ExtractPayload_UnwrapsSnsEnvelopeWithLowercaseFields()
    {
        const string body = """
            {
              "type": "Notification",
              "message": "{\"symbol\":\"PETR4\",\"price\":31.42}"
            }
            """;

        var payload = global::SqsMessageBody.ExtractPayload(body);

        Assert.Equal("""{"symbol":"PETR4","price":31.42}""", payload);
    }

    [Fact]
    public void ExtractPayload_UnwrapsStructuredSnsMessage()
    {
        const string body = """
            {
              "Type": "Notification",
              "Message": { "symbol": "PETR4", "price": 31.42 }
            }
            """;

        var payload = global::SqsMessageBody.ExtractPayload(body);

        Assert.Equal("""{ "symbol": "PETR4", "price": 31.42 }""", payload);
    }

    [Fact]
    public void ExtractPayload_UnwrapsArraySnsMessage()
    {
        const string body = """
            {
              "Type": "Notification",
              "Message": [{ "symbol": "PETR4", "price": 31.42 }]
            }
            """;

        var payload = global::SqsMessageBody.ExtractPayload(body);

        Assert.Equal("""[{ "symbol": "PETR4", "price": 31.42 }]""", payload);
    }

    [Fact]
    public void ExtractPayload_LeavesMalformedJsonAsIs()
    {
        const string body = "{";

        var payload = global::SqsMessageBody.ExtractPayload(body);

        Assert.Equal(body, payload);
    }
}
