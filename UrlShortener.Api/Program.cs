using UrlShortener.Api.Repository;

var builder = WebApplication.CreateBuilder(args);

var mappingsFileConfig = builder.Configuration["MappingsFile"] ?? "urls.txt";
var mappingsFilePath = Path.IsPathRooted(mappingsFileConfig)
    ? mappingsFileConfig
    : Path.Combine(builder.Environment.ContentRootPath, mappingsFileConfig);

builder.Services.AddSingleton<IUrlRepository>(sp => new FileUrlRepository(mappingsFilePath, sp.GetRequiredService<ILogger<FileUrlRepository>>()));

var app = builder.Build();

app.MapGet("/{code}", (string code, IUrlRepository repo, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(code))
    {
        logger.LogWarning("Empty short code requested.");
        return Results.BadRequest("Code is required.");
    }

    if (!repo.TryGetLongUrl(code, out var longUrl))
    {
        logger.LogInformation("Short code '{Code}' not found.", code);
        return Results.NotFound($"Short code '{code}' not found.");
    }

    logger.LogInformation("Redirecting code '{Code}' -> {Url}", code, longUrl);
    return Results.Redirect(longUrl);
});

app.Run();

public partial class Program { }
