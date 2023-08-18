using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var address = context.Connection.RemoteIpAddress;
            _logger.LogInformation("Request {Method} {Url} invoked from: {address}", context.Request.Method, context.Request.Path, address);

            try
            {
                await _next(context);
            }
            finally
            {
                _logger.LogInformation("Request {Method} {Url} for {address} done with status code {StatusCode}", context.Request.Method, context.Request.Path, address, context.Response.StatusCode);
            }
        }
    }
}
