using System.Security.Claims;
using IdentityService.Api.Extensions;
using IdentityService.Api.Middleware;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using IdentityService.Application.UseCases.Users;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// 1️⃣ SERVICIOS BASE
// ======================================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ======================================================
// 2️⃣ SWAGGER + JWT
// ======================================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityService API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT en formato: Bearer {token}",
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
});

// ======================================================
// 3️⃣ BASE DE DATOS
// ======================================================
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("default"))
);

// ======================================================
// 4️⃣ SEGURIDAD
// ======================================================

// JWT (⚠️ REGISTRA Authentication + Bearer UNA SOLA VEZ)
builder.Services.AddJwt(builder.Configuration);

// Acceso a HttpContext
builder.Services.AddHttpContextAccessor();

// Políticas RBAC dinámicas desde BD
builder.Services.AddDynamicPolicies();

// CORS
builder.Services.AddCorsPolicy(builder.Configuration);

// ======================================================
// 5️⃣ REPOSITORIOS
// ======================================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<ISystemSettingsRepository, SystemSettingsRepository>();

// ======================================================
// 6️⃣ CASOS DE USO / HANDLERS
// ======================================================
builder.Services.AddScoped<RegisterUserHandler>();

builder.Services.AddScoped<IEnableMfaUseCase, EnableMfaUseCase>();
builder.Services.AddScoped<IConfirmMfaUseCase, ConfirmMfaUseCase>();
builder.Services.AddScoped<IDisableMfaUseCase, DisableMfaUseCase>();

builder.Services.AddScoped<IMfaHandler, MfaHandler>();
builder.Services.AddScoped<IMfaService, MfaService>();

// ======================================================
// 7️⃣ SERVICIOS DE APLICACIÓN
// ======================================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();

// ======================================================
// 8️⃣ BUILD
// ======================================================
var app = builder.Build();

// ======================================================
// 9️⃣ MIDDLEWARE
// ======================================================

// Swagger solo en Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Routing
app.UseRouting();

// Manejo global de excepciones (OWASP)
app.UseMiddleware<ExceptionMiddleware>();

// Auditoría y logging de seguridad
app.UseMiddleware<AuditMiddleware>();

// CORS
app.UseCors("DefaultPolicy");

// Autenticación
app.UseAuthentication();

// Validación JWT extra (TokenVersion, usuario bloqueado, etc.)
app.UseMiddleware<JwtValidationMiddleware>();

// Autorización
app.UseAuthorization();

// ======================================================
// 🔐 VALIDACIÓN TOKEN VERSION (INVALIDACIÓN JWT)
// ======================================================
app.Use(
    async (context, next) =>
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tokenVersionClaim = context.User.FindFirst("tokenVersion")?.Value;

            if (userIdClaim == null || tokenVersionClaim == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var userId = Guid.Parse(userIdClaim);
            var tokenVersion = int.Parse(tokenVersionClaim);

            var userRepo = context.RequestServices.GetRequiredService<IUserRepository>();
            var user = await userRepo.GetByIdAsync(userId);

            if (user == null || user.TokenVersion != tokenVersion || user.IsLocked)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }

        await next();
    }
);

// ======================================================
// 🔁 SEED ADMIN INICIAL
// ======================================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await IdentityDbSeeder.SeedAsync(dbContext);
}

// ======================================================
// 🔚 ENDPOINTS
// ======================================================
app.MapControllers();
app.Run();
