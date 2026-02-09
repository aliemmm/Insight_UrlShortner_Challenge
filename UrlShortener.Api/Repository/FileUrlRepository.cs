using System.Collections.Concurrent;

namespace UrlShortener.Api.Repository;

public class FileUrlRepository : IUrlRepository
{
    private readonly ConcurrentDictionary<string, string> _mappings = new();
    private readonly ILogger<FileUrlRepository> _logger;

    public FileUrlRepository(string filePath, ILogger<FileUrlRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path must be provided.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("URL mapping file not found.", filePath);

        LoadUrlMappings(filePath);
    }

    private void LoadUrlMappings(string filePath)
    {
        _logger.LogInformation("Loading URL mappings from {Path}", filePath);

        foreach (var rawLine in File.ReadLines(filePath))
        {
            var line = rawLine?.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            // Expect format: <id> <longUrl> <code>
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                _logger.LogDebug("Skipping invalid mapping line: {Line}", line);
                continue;
            }

            var code = parts[^1];
            var longUrlCandidate = string.Join(' ', parts.Skip(1).Take(parts.Length - 2));

            if (!Uri.TryCreate(longUrlCandidate, UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                _logger.LogWarning("Skipping mapping with invalid URL '{Url}' for code '{Code}'", longUrlCandidate, code);
                continue;
            }

            var longUrl = uri.ToString();

            _mappings[code] = longUrl;
            _logger.LogDebug("Loaded mapping {Code} -> {Url}", code, longUrl);
        }

        _logger.LogInformation("Finished loading {Count} mappings", _mappings.Count);
    }

    public bool TryGetLongUrl(string code, out string longUrl)
        => _mappings.TryGetValue(code, out longUrl!);
}
