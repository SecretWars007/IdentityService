using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Infrastructure.Auth
{
    public class JwtSettings(string issuer, string audience, string secret, int expirationMinutes)
    {
        public string Issuer { get; set; } = issuer;
        public string Audience { get; set; } = audience;
        public string Secret { get; set; } = secret;
        public int ExpirationMinutes { get; set; } = expirationMinutes;
    }
}
