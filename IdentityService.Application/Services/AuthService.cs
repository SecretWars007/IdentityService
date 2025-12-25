using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IMfaService _mfaService;
        private readonly IAuditLogRepository _auditLogRepository;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IMfaService mfaService,
            IAuditLogRepository auditLogRepository
        )
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _mfaService = mfaService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<LoginResponse> LoginAsync(
            string email,
            string password,
            string? mfaCode,
            string ip
        )
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    await _auditLogRepository.LogAsync(
                        new AuditLog(
                            Guid.Empty,
                            "LoginFailed",
                            $"Intento de login fallido para email {email}",
                            ip
                        )
                    );
                    throw new Exception("Credenciales inválidas"); // mensaje genérico
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    await _auditLogRepository.LogAsync(
                        new AuditLog(user.Id, "LoginFailed", "Contraseña incorrecta", ip)
                    );
                    throw new Exception("Credenciales inválidas");
                }

                // Si MFA está configurado pero no confirmado
                if (user.Mfa != null && !user.Mfa.Confirmed)
                {
                    await _auditLogRepository.LogAsync(
                        new AuditLog(user.Id, "LoginFailed", "MFA no confirmado", ip)
                    );
                    throw new Exception("Debe confirmar MFA antes de iniciar sesión");
                }

                // Si MFA confirmado → validar código
                if (user.Mfa != null && user.Mfa.Confirmed)
                {
                    if (
                        string.IsNullOrWhiteSpace(mfaCode)
                        || !_mfaService.ValidateMfa(user.Mfa.Secret, mfaCode)
                    )
                    {
                        await _auditLogRepository.LogAsync(
                            new AuditLog(user.Id, "LoginFailed", "Código MFA inválido", ip)
                        );
                        throw new Exception("Credenciales inválidas"); // mensaje genérico
                    }
                }

                // Login exitoso → generar token
                var token = _jwtTokenGenerator.Generate(user);

                await _auditLogRepository.LogAsync(
                    new AuditLog(user.Id, "LoginSuccess", "Login exitoso", ip)
                );

                return new LoginResponse
                {
                    Token = token,
                    UserId = user.Id,
                    Email = user.Email,
                    Role = user.Roles.FirstOrDefault()?.Role.Name ?? "User",
                };
            }
            catch (Exception)
            {
                // Aquí se puede agregar un log adicional global si quieres
                // throw new Exception(ex.Message); // no exponer mensajes internos
                throw; // deja que los mensajes genéricos ya establecidos se propaguen
            }
        }

        public (string secret, string qrBase64) EnableMfa(User user)
        {
            var (secret, qrBase64) = _mfaService.GenerateMfa(user);
            user.SetupMfa(secret);
            return (secret, qrBase64);
        }

        public void ConfirmMfa(User user, string code)
        {
            if (!_mfaService.ValidateMfa(user.Mfa!.Secret, code))
                throw new Exception("Código MFA inválido");

            user.ConfirmMfa();
        }

        public void DisableMfa(User user)
        {
            user.DisableMfa();
        }
    }
}
