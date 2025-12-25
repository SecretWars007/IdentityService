using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Infrastructure.Auth
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _config;

        public JwtTokenGenerator(IConfiguration config)
        {
            _config = config;
        }

        public string Generate(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var claims = new List<Claim>
            {
                // 🔐 Claim estándar usado por ASP.NET y por tu MFA
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                // Email
                new Claim(ClaimTypes.Email, user.Email),
                // Sub estándar JWT (compatibilidad)
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                // JTI para trazabilidad / revocación
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // 🔥 ROLES (CRÍTICO para [Authorize(Roles = "...")])
            foreach (var userRole in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
            }

            // 🔹 PERMISOS (para policies dinámicas)
            foreach (
                var permission in user
                    .Roles.SelectMany(r => r.Role.Permissions)
                    .Select(p => p.Permission.Code)
                    .Distinct()
            )
            {
                claims.Add(new Claim("permission", permission));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["jwt:secret"]!));

            var token = new JwtSecurityToken(
                issuer: _config["jwt:issuer"],
                audience: _config["jwt:audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
