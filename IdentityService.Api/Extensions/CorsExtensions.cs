using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Api.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsPolicy(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var allowedOrigins =
                configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? Array.Empty<string>();

            services.AddCors(options =>
            {
                options.AddPolicy(
                    "DefaultPolicy",
                    policy =>
                    {
                        if (allowedOrigins.Length > 0)
                        {
                            policy
                                .WithOrigins(allowedOrigins)
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .AllowCredentials();
                        }
                        else
                        {
                            // Desarrollo: permitir todo
                            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                        }
                    }
                );
            });

            return services;
        }
    }
}
