using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.Services
{
    public class MfaHandler : IMfaHandler
    {
        private readonly IMfaService _mfaService;
        private readonly IAuditLogRepository _auditLogRepository;

        public MfaHandler(IMfaService mfaService, IAuditLogRepository auditLogRepository)
        {
            _mfaService = mfaService;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<EnableMfaResponse> EnableMfa(User user, string ipAddress)
        {
            if (user.IsMfaEnabled)
                throw new InvalidOperationException("MFA ya está habilitado");

            var (secret, qrBase64) = _mfaService.GenerateMfa(user);
            user.SetupMfa(secret);

            await _auditLogRepository.LogAsync(
                new AuditLog(user.Id, "MFA habilitado", "Éxito", ipAddress)
            );

            return new EnableMfaResponse(secret, qrBase64);
        }

        public async Task ConfirmMfa(User user, string code, string ipAddress)
        {
            if (!user.IsMfaEnabled)
                throw new InvalidOperationException("MFA no habilitado");

            if (!_mfaService.ValidateMfa(user.Mfa!.Secret, code))
                throw new InvalidOperationException("Código MFA inválido");

            user.Mfa!.Confirm();

            await _auditLogRepository.LogAsync(
                new AuditLog(user.Id, "MFA confirmado", "Éxito", ipAddress)
            );
        }

        public async Task DisableMfa(User user, string ipAddress)
        {
            if (!user.IsMfaEnabled)
                throw new InvalidOperationException("MFA no habilitado");

            user.DisableMfa();

            await _auditLogRepository.LogAsync(
                new AuditLog(user.Id, "MFA deshabilitado", "Éxito", ipAddress)
            );
        }
    }
}
