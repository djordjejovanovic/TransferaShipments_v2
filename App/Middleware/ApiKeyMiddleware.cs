namespace TransferaShipments.App.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(
        RequestDelegate next,
        IConfiguration configuration,
        ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip enforcement for public endpoints
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var configuredApiKey = _configuration["ApiKey"];

        // If no API key is configured, skip enforcement
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            _logger.LogWarning("No API key configured. Skipping API key enforcement.");
            await _next(context);
            return;
        }

        // Try to get API key from header first, then query string
        var providedApiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            providedApiKey = context.Request.Query["api_key"].FirstOrDefault();
        }

        // Validate API key
        if (string.IsNullOrWhiteSpace(providedApiKey) || !string.Equals(providedApiKey, configuredApiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning("Unauthorized request to {Path}. Invalid or missing API key.", path);
            context.Response.StatusCode = 401;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAsync("Unauthorized: Invalid or missing API key.");
            return;
        }

        // API key is valid, proceed with the request
        await _next(context);
    }
}
