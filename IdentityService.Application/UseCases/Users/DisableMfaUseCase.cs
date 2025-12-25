using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.UseCases.Users
{
    public class DisableMfaUseCase : IDisableMfaUseCase
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public DisableMfaUseCase(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task ExecuteAsync(User user, string ip)
        {
            user.DisableMfa();
            await _auditLogRepository.LogAsync(
                new AuditLog(user.Id, "DisableMfa", "MFA deshabilitado", ip)
            );
        }
    }
}
