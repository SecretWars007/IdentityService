using System.Security.Claims;
using System.Text;
using IdentityService.Application.Interfaces;
using IdentityService.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Api.Extensions
{
    public static class JwtExtensions
    {
        public static IServiceCollection AddJwt(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();

            if (jwtSettings is null)
            {
                throw new InvalidOperationException(
                    "JwtSettings no está configurado en appsettings.json"
                );
            }

            ValidateJwtSettings(jwtSettings);

            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.Secret)
                        ),
                        RoleClaimType = ClaimTypes.Role, // 🔥 ESTA LÍNEA
                        NameClaimType = ClaimTypes.NameIdentifier,

                        ClockSkew = TimeSpan.Zero,
                    };

                    // 🔐 INVALIDACIÓN CENTRALIZADA DE JWT
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            var logger = context
                                .HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                                .CreateLogger("JwtValidation");

                            var userIdClaim = context
                                .Principal?.FindFirst(ClaimTypes.NameIdentifier)
                                ?.Value;

                            var tokenVersionClaim = context
                                .Principal?.FindFirst("tokenVersion")
                                ?.Value;

                            if (userIdClaim is null || tokenVersionClaim is null)
                            {
                                logger.LogWarning("JWT inválido: claims faltantes");
                                context.Fail("Token inválido");
                                return;
                            }

                            var userRepository =
                                context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();

                            var user = await userRepository.GetByIdAsync(Guid.Parse(userIdClaim));

                            if (user is null)
                            {
                                logger.LogWarning("JWT inválido: usuario no existe");
                                context.Fail("Token inválido");
                                return;
                            }

                            if (user.IsLocked)
                            {
                                logger.LogWarning(
                                    "JWT rechazado: usuario bloqueado {UserId}",
                                    user.Id
                                );
                                context.Fail("Usuario bloqueado");
                                return;
                            }

                            if (user.TokenVersion != int.Parse(tokenVersionClaim))
                            {
                                logger.LogWarning(
                                    "JWT invalidado por TokenVersion {UserId}",
                                    user.Id
                                );
                                context.Fail("Token invalidado");
                            }
                        },
                    };
                });

            return services;
        }

        // 🛡️ VALIDACIÓN DE CONFIGURACIÓN (OWASP)
        private static void ValidateJwtSettings(JwtSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.Secret))
                throw new InvalidOperationException("JwtSettings.Secret es obligatorio");

            if (settings.Secret.Length < 32)
                throw new InvalidOperationException(
                    "JwtSettings.Secret debe tener al menos 32 caracteres"
                );

            if (string.IsNullOrWhiteSpace(settings.Issuer))
                throw new InvalidOperationException("JwtSettings.Issuer es obligatorio");

            if (string.IsNullOrWhiteSpace(settings.Audience))
                throw new InvalidOperationException("JwtSettings.Audience es obligatorio");
        }
    }
}
