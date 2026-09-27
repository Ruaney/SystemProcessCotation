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

            if (!TryGetPropertyIgnoreCase(document.RootElement, "Type", out var type)
                || type.ValueKind != JsonValueKind.String
                || !string.Equals(type.GetString()?.Trim(), "Notification", StringComparison.OrdinalIgnoreCase))
            {
                return body;
            }

            if (TryGetPropertyIgnoreCase(document.RootElement, "Message", out var message))
            {
                return message.ValueKind switch
                {
                    JsonValueKind.String => NormalizeStringPayload(message.GetString(), body),
                    JsonValueKind.Object or JsonValueKind.Array => message.GetRawText(),
                    _ => body
                };
            }
        }
        catch (JsonException)
        {
            return body;
        }

        return body;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string NormalizeStringPayload(string? payload, string fallback)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return fallback;
        }

        var trimmed = payload.Trim();
        if (!IsJsonStringLiteral(trimmed))
        {
            return payload;
        }

        try
        {
            return JsonSerializer.Deserialize<string>(trimmed) ?? payload;
        }
        catch (JsonException)
        {
            return payload;
        }
    }

    private static bool IsJsonStringLiteral(string value) =>
        value.Length >= 2
        && value[0] == '"'
        && value[^1] == '"';
}
