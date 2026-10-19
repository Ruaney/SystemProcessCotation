using System.Text.Json;

internal static class SqsMessageBody
{
    public static string ExtractPayload(string? body)
    {
        if (body is null)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return body;
        }

        var payload = body;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var extractedPayload = TryExtractSnsPayload(payload);
            if (extractedPayload is null)
            {
                return payload;
            }

            payload = extractedPayload;
        }

        return payload;
    }

    private static string? TryExtractSnsPayload(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !TryGetPropertyIgnoreCase(document.RootElement, "Type", out var type)
                || type.ValueKind != JsonValueKind.String
                || !string.Equals(type.GetString()?.Trim(), "Notification", StringComparison.OrdinalIgnoreCase)
                || !TryGetPropertyIgnoreCase(document.RootElement, "Message", out var message))
            {
                return null;
            }

            return message.ValueKind switch
            {
                JsonValueKind.String => NormalizeStringPayload(message.GetString(), body),
                JsonValueKind.Object or JsonValueKind.Array => message.GetRawText(),
                _ => body
            };
        }
        catch (JsonException)
        {
            return null;
        }
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
        var normalized = payload;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var candidate = normalized.Trim();
            if (!IsJsonStringLiteral(candidate))
            {
                return normalized;
            }

            try
            {
                normalized = JsonSerializer.Deserialize<string>(candidate) ?? normalized;
            }
            catch (JsonException)
            {
                return normalized;
            }
        }

        return normalized;
    }

    private static bool IsJsonStringLiteral(string value) =>
        value.Length >= 2
        && value[0] == '"'
        && value[^1] == '"';
}
