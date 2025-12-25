using IdentityService.Api.Extensions;
using IdentityService.Api.Middleware;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using IdentityService.Application.UseCases.Users;
using IdentityService.Infrastructure.Auth;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// =====================================
// 1️⃣ Servicios base
// =====================================
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityService API", Version = "v1" });

    // Configuración de JWT Bearer
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa tu token JWT generado después del login",
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    var securityRequirement = new OpenApiSecurityRequirement
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
            new string[] { }
        },
    };

    c.AddSecurityRequirement(securityRequirement);
});

// =====================================
// 2️⃣ Base de datos
// =====================================
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("default"))
);

// =====================================
// 3️⃣ Seguridad
// =====================================

// JWT + Refresh Tokens + MFA
builder.Services.AddJwt(builder.Configuration);

// 👈 Registrar IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// RBAC dinámico (policies desde DB)
builder.Services.AddDynamicPolicies();

// CORS
builder.Services.AddCorsPolicy(builder.Configuration);

// =====================================
// 4️⃣ Repositorios
// =====================================
builder.Services.AddScoped<RegisterUserHandler>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IMfaHandler, MfaHandler>();
builder.Services.AddScoped<IEnableMfaUseCase, EnableMfaUseCase>();
builder.Services.AddScoped<IConfirmMfaUseCase, ConfirmMfaUseCase>();
builder.Services.AddScoped<IDisableMfaUseCase, DisableMfaUseCase>();
builder.Services.AddScoped<IMfaService, MfaService>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

// =====================================
// 5️⃣ Servicios de aplicación
// =====================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

// =====================================
// 6️⃣ Auth pipeline
// =====================================
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// =====================================
// 7️⃣ Build
// =====================================
var app = builder.Build();

// =====================================
// 8️⃣ Middleware
// =====================================

// Swagger SOLO en Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Orden recomendado: Routing primero
app.UseRouting();

// 🔹 Middleware de excepciones mejorado
app.UseMiddleware<ExceptionMiddleware>();

// 🔹 Middleware de auditoría OWASP
//   Nota: AuditMiddleware resuelve IAuditLogRepository dentro de Invoke
app.UseMiddleware<AuditMiddleware>();

// CORS
app.UseCors("DefaultPolicy");

// Auth
app.UseAuthentication();
app.UseAuthorization();

// =====================================
// 9️⃣ Seed usuario ADMIN inicial
// =====================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await IdentityDbSeeder.SeedAsync(dbContext);
}

// =====================================
// 10️⃣ Endpoints
// =====================================
app.MapControllers();

app.Run();
