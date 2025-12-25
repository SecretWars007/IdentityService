using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using IdentityService.Application.Exceptions;

namespace IdentityService.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env
        )
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (UnauthorizedException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Unauthorized access attempt at {Path}",
                    context.Request.Path
                );
                await WriteResponseAsync(context, HttpStatusCode.Unauthorized, "No autorizado");
            }
            catch (ForbiddenException ex)
            {
                _logger.LogWarning(ex, "Forbidden access attempt at {Path}", context.Request.Path);
                await WriteResponseAsync(context, HttpStatusCode.Forbidden, "Acceso prohibido");
            }
            catch (Exception ex)
            {
                // Log completo del error
                _logger.LogError(ex, "Unhandled exception at {Path}", context.Request.Path);

                if (_env.IsDevelopment())
                {
                    // Desarrollo: detalle completo de la excepción
                    await WriteResponseAsync(
                        context,
                        HttpStatusCode.InternalServerError,
                        ex.Message,
                        ex.StackTrace,
                        ex.GetType().Name
                    );
                }
                else
                {
                    // Producción: mensaje genérico
                    await WriteResponseAsync(
                        context,
                        HttpStatusCode.InternalServerError,
                        "Error interno del servidor"
                    );
                }
            }
        }

        private static async Task WriteResponseAsync(
            HttpContext context,
            HttpStatusCode statusCode,
            string message,
            string? stackTrace = null,
            string? exceptionType = null
        )
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var payload = new
            {
                message,
                stackTrace,
                type = exceptionType,
            };
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // ✅ Reemplaza IgnoreNullValues
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, options));
        }
    }
}
