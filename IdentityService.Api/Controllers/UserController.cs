using System.Security.Claims;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Application.UseCases.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly Application.Interfaces.IAuthorizationService _authorizationService;
        private readonly RegisterUserHandler _registerHandler;
        private readonly IUserRepository _userRepository;

        public UserController(
            Application.Interfaces.IAuthorizationService authorizationService,
            RegisterUserHandler registerHandler,
            IUserRepository userRepository
        )
        {
            _authorizationService = authorizationService;
            _registerHandler = registerHandler;
            _userRepository = userRepository;
        }

        // -------------------------------
        // 1️⃣ Registrar un nuevo usuario
        // -------------------------------
        [HttpPost("register")]
        [Authorize(Roles = "ADMINISTRADOR,SUPERVISOR")] // Solo ciertos roles pueden crear usuarios
        public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
        {
            try
            {
                var useCase = new RegisterUserUseCase(request);
                await _registerHandler.ExecuteAsync(useCase);
                return Ok(new { message = "Usuario registrado correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // ----------------------------------------
        // 2️⃣ Obtener perfil completo de un usuario
        // ----------------------------------------
        [HttpGet("{id}")]
        [Authorize] // Cualquier usuario autenticado puede solicitar, pero validamos propietario
        public async Task<IActionResult> GetUser(Guid id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            _authorizationService.EnsureOwner(id, userId); // Anti-IDOR

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { error = "Usuario no encontrado" });

            var dto = new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                DocumentNumber = user.Profile?.DocumentNumber,
                Address = user.Profile?.Address,
                Phone = user.Profile?.Phone,
                BirthDate = user.BirthDate,
                PhotoUrl = user.Profile?.PhotoUrl,
                Roles = user.Roles.Select(r => r.Role.Name).ToList(),
            };

            return Ok(dto);
        }
    }
}
