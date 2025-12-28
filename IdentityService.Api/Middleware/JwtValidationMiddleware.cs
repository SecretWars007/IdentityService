using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityService.Application.Interfaces;

namespace IdentityService.Api.Middleware
{
    public class JwtValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(
            HttpContext context,
            IUserRepository userRepository,
            ILogger<JwtValidationMiddleware> logger
        )
        {
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokenVersionClaim = context.User.FindFirst("token_version")?.Value;

            if (
                !Guid.TryParse(userId, out var id)
                || !int.TryParse(tokenVersionClaim, out var tokenVersion)
            )
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var user = await userRepository.GetByIdAsync(id);

            if (user == null || user.IsDisabled || user.TokenVersion != tokenVersion)
            {
                logger.LogWarning("JWT invalidated for user {UserId}", id);

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            await _next(context);
        }
    }
}
