using System.Security.Claims;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Application.UseCases.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/mfa")]
[Authorize]
public class MfaController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IEnableMfaUseCase _enableMfaUseCase;
    private readonly IConfirmMfaUseCase _confirmMfaUseCase;
    private readonly IDisableMfaUseCase _disableMfaUseCase;

    public MfaController(
        IUserRepository userRepository,
        IEnableMfaUseCase enableMfaUseCase,
        IConfirmMfaUseCase confirmMfaUseCase,
        IDisableMfaUseCase disableMfaUseCase
    )
    {
        _userRepository = userRepository;
        _enableMfaUseCase = enableMfaUseCase;
        _confirmMfaUseCase = confirmMfaUseCase;
        _disableMfaUseCase = disableMfaUseCase;
    }

    [HttpPost("enable")]
    [Authorize]
    public async Task<ActionResult<EnableMfaResponse>> EnableMfa()
    {
        // 1️⃣ Validar claim estándar del JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim is null)
            return Unauthorized(
                new { error = "Token JWT inválido", detail = "No contiene el claim NameIdentifier" }
            );

        // 2️⃣ Validar formato del UserId
        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized(
                new { error = "Token JWT inválido", detail = "UserId no es un GUID válido" }
            );

        // 3️⃣ Obtener usuario desde base de datos
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFound(new { error = "Usuario no encontrado" });

        // 4️⃣ Validar estado actual del MFA
        if (user.IsMfaEnabled)
            return BadRequest(new { error = "MFA ya está habilitado para este usuario" });

        // 5️⃣ Obtener IP (auditoría)
        var ip =
            HttpContext.Connection.RemoteIpAddress?.ToString()
            ?? HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? "unknown";

        // 6️⃣ Ejecutar caso de uso (Application Layer)
        var result = await _enableMfaUseCase.ExecuteAsync(user, ip);

        // 7️⃣ Retornar QR + secret
        return Ok(new EnableMfaResponse(result.Secret, result.QrBase64));
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmMfa([FromBody] ConfirmMfaRequest request)
    {
        // 1️⃣ Validación defensiva del body
        if (request is null)
            return BadRequest("Body inválido o vacío");

        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("Código MFA requerido");

        // 2️⃣ Validación estricta del JWT
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim is null)
            return Unauthorized("JWT inválido o sin NameIdentifier");

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized("UserId inválido en el token");

        // 3️⃣ Cargar usuario
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return NotFound("Usuario no encontrado");
        Console.WriteLine(user.Mfa == null ? "MFA es null" : "MFA existe");
        // 4️⃣ Validar estado del MFA
        if (user.Mfa == null)
            return Conflict("MFA no inicializado");
        if (user.Mfa.Confirmed)
            return Conflict("MFA ya fue confirmado");

        // 5️⃣ Obtener IP
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // 6️⃣ Ejecutar caso de uso
        await _confirmMfaUseCase.ExecuteAsync(user, request.Code, ip);

        return Ok(new { message = "MFA confirmado correctamente" });
    }

    [HttpPost("disable")]
    public async Task<IActionResult> DisableMfa()
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return NotFound();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _disableMfaUseCase.ExecuteAsync(user, ip);

        return Ok(new { message = "MFA deshabilitado correctamente" });
    }
}
