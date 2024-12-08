using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace API.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

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

                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    await _next(context); // Process request

                    // Log response
                    stopwatch.Stop();
                    var responseLog = await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds);
                    _logger.LogInformation("Response: {ResponseLog}", responseLog);

                    await responseBody.CopyToAsync(originalResponseBodyStream);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "An exception occurred. CorrelationId: {CorrelationId}", correlationId);
                throw; // Let the exception propagate to other middleware or response utils
            }
        }

        private async Task<string> LogRequestAsync(HttpContext context, string correlationId)
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
                Scheme = context.Request.Scheme,
                Host = context.Request.Host,
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                Method = context.Request.Method,
                Headers = context.Request.Headers,
                Body = requestBody
            };

            return JsonSerializer.Serialize(log, new JsonSerializerOptions { WriteIndented = true });
        }

        private async Task<string> LogResponseAsync(HttpContext context, string correlationId, long elapsedMilliseconds)
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
                StatusCode = context.Response.StatusCode,
                Headers = context.Response.Headers,
                Body = responseBody,
                ExecutionTimeMs = elapsedMilliseconds
            };

            return JsonSerializer.Serialize(log, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
