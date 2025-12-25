using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace IdentityService.Api.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
        {
            // Resolver el servicio scoped dentro del scope de la petición
            var auditRepo = serviceProvider.GetRequiredService<IAuditLogRepository>();

            await _next(context);

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            await auditRepo.LogAsync(
                new AuditLog(
                    userId != null ? Guid.Parse(userId) : null,
                    $"{context.Request.Method} {context.Request.Path}",
                    context.Response.StatusCode.ToString(),
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
                )
            );
        }
    }
}
