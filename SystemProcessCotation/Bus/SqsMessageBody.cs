using System.Text.Json;

internal static class SqsMessageBody
{
    public static string ExtractPayload(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return body;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return body;
            }

            if (!document.RootElement.TryGetProperty("Type", out var type)
                || !string.Equals(type.GetString(), "Notification", StringComparison.OrdinalIgnoreCase))
            {
                return body;
            }

            if (document.RootElement.TryGetProperty("Message", out var message)
                && message.ValueKind == JsonValueKind.String)
            {
                return message.GetString() ?? body;
            }
        }
        catch (JsonException)
        {
            return body;
        }

        return body;
    }
}
