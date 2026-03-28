using Horscht.Contracts.Services;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Horscht.Importer.Services;

internal class AiSongMetadataExtractionService : ISongMetadataExtractionService
{
    private const string ApiVersion = "2024-10-21";

    private readonly HttpClient _httpClient;
    private readonly AzureOpenAIOptions _options;
    private readonly ILogger<AiSongMetadataExtractionService> _logger;

    public AiSongMetadataExtractionService(
        HttpClient httpClient,
        IOptions<AzureOpenAIOptions> options,
        ILogger<AiSongMetadataExtractionService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SongMetadataExtractionResult?> TryExtractFromFilename(string filename, CancellationToken cancellationToken)
    {
        var filenameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
        if (string.IsNullOrWhiteSpace(filenameWithoutExtension))
        {
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, BuildChatCompletionsUri())
            {
                Content = JsonContent.Create(new
                {
                    messages = new object[]
                    {
                        new
                        {
                            role = "system",
                            content = "Extract music metadata from filenames. Return only JSON with keys artist and title. Use null when unknown."
                        },
                        new
                        {
                            role = "user",
                            content = $"Filename: {filenameWithoutExtension}"
                        }
                    },
                    temperature = 0,
                    response_format = new
                    {
                        type = "json_object"
                    }
                })
            };

            request.Headers.Add("api-key", _options.ApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "AI metadata extraction failed for '{Filename}'. Status: {StatusCode}. Body: {Body}",
                    filename,
                    (int)response.StatusCode,
                    errorBody);

                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var content = ExtractContent(responseBody);

            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            var jsonContent = StripCodeFences(content);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return null;
            }

            using var document = JsonDocument.Parse(jsonContent);
            var artist = TryGetNormalizedString(document.RootElement, "artist");
            var title = TryGetNormalizedString(document.RootElement, "title");

            if (string.IsNullOrWhiteSpace(artist) && string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            return new SongMetadataExtractionResult(artist, title);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI metadata extraction threw an exception for '{Filename}'.", filename);
            return null;
        }
    }

    private Uri BuildChatCompletionsUri()
    {
        var endpoint = _options.Endpoint.EndsWith('/') ? _options.Endpoint : $"{_options.Endpoint}/";
        var path = $"openai/deployments/{Uri.EscapeDataString(_options.DeploymentName)}/chat/completions?api-version={ApiVersion}";
        return new Uri(new Uri(endpoint), path);
    }

    private static string? ExtractContent(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);

        if (!document.RootElement.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
        {
            return null;
        }

        var firstChoice = choices[0];
        if (!firstChoice.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("content", out var contentElement) ||
            contentElement.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return contentElement.GetString();
    }

    private static string StripCodeFences(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var lines = trimmed.Split('\n');
        if (lines.Length <= 2)
        {
            return trimmed;
        }

        var builder = new StringBuilder();

        for (var i = 1; i < lines.Length - 1; i++)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(lines[i]);
        }

        return builder.ToString().Trim();
    }

    private static string? TryGetNormalizedString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var text = value.GetString();
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }
}

