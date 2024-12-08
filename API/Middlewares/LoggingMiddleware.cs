using System.Diagnostics;
using System.Text.Json;

namespace CompanyDirectory.API.Middlewares
{
    public class LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true
        };

        private readonly RequestDelegate _next = next;
        private readonly ILogger<LoggingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Log request
                var requestLog = await LogRequestAsync(context, correlationId);
                _logger.LogInformation("Request: {RequestLog}", requestLog);

                // Capture response
                var originalResponseBodyStream = context.Response.Body;

                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await _next(context); // Process request

                // Log response
                stopwatch.Stop();
                var responseLog = await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);
                _logger.LogInformation("Response: {ResponseLog}", responseLog);

                await responseBody.CopyToAsync(originalResponseBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An exception occurred. CorrelationId: {CorrelationId}", correlationId);
                throw; // Let the exception propagate to other middleware or response utils
            }
        }

        private static async Task<string> LogRequestAsync(HttpContext context, string correlationId)
        {
            context.Request.EnableBuffering();

            var requestBody = string.Empty;
            if (context.Request.ContentLength > 0 && context.Request.ContentType?.Contains("application/json") == true)
            {
                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            var log = new
            {
                CorrelationId = correlationId,
                context.Request.Scheme,
                context.Request.Host,
                context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                context.Request.Method,
                context.Request.Headers,
                Body = requestBody
            };


            return JsonSerializer.Serialize(log, _jsonSerializerOptions);
        }

        private static async Task<string> LogResponseAsync(HttpContext context, string correlationId, long elapsedMilliseconds)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            string responseBody;
            try
            {
                responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            }
            catch (ObjectDisposedException)
            {
                responseBody = "<Stream Closed>";
            }

            context.Response.Body.Seek(0, SeekOrigin.Begin);

            var log = new
            {
                CorrelationId = correlationId,
                context.Response.StatusCode,
                context.Response.Headers,
                Body = responseBody,
                ExecutionTimeMs = elapsedMilliseconds
            };

            return JsonSerializer.Serialize(log, _jsonSerializerOptions);
        }
    }
}
