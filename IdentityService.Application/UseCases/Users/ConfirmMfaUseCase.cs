using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.UseCases.Users
{
    public class ConfirmMfaUseCase : IConfirmMfaUseCase
    {
        private readonly IMfaService _mfaService;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public ConfirmMfaUseCase(
            IMfaService mfaService,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository
        )
        {
            _mfaService = mfaService;
            _userRepository = userRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task ExecuteAsync(User user, string code, string ip)
        {
            if (user.Mfa == null)
                throw new InvalidOperationException("MFA no inicializado");

            if (user.Mfa.Enabled)
                throw new InvalidOperationException("MFA deshabilitado");

            if (user.Mfa.Confirmed)
                throw new InvalidOperationException("MFA ya confirmado");

            if (!_mfaService.ValidateMfa(user.Mfa.Secret, code))
                throw new InvalidOperationException("Código MFA inválido");

            user.Mfa.Confirm();

            await _userRepository.UpdateAsync(user);

            await _auditLogRepository.LogAsync(
                new AuditLog(user.Id, "ConfirmMfa", "MFA confirmado", ip)
            );
        }
    }
}
