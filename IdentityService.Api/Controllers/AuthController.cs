using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IAuthService authService, IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var ip =
                    _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown";
                var response = await _authService.LoginAsync(
                    request.Email,
                    request.Password,
                    request.MfaCode,
                    ip
                );
                return Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                // Mensaje genérico al usuario, registro detallado ya se hace en el servicio
                return Unauthorized(new { Message = "Credenciales inválidas" });
            }
            catch (Exception)
            {
                // Errores inesperados
                return StatusCode(500, new { Message = "Error interno del servidor" });
            }
        }
    }
}
