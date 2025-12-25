using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityService.Application.DTOs;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.UseCases.Users
{
    public class EnableMfaUseCase : IEnableMfaUseCase
    {
        private readonly IMfaService _mfaService;
        private readonly IUserRepository _userRepository;

        private readonly IAuditLogRepository _auditLogRepository;

        public EnableMfaUseCase(
            IMfaService mfaService,
            IAuditLogRepository auditLogRepository,
            IUserRepository userRepository
        )
        {
            _mfaService = mfaService;
            _auditLogRepository = auditLogRepository;
            _userRepository = userRepository;
        }

        public async Task<EnableMfaResponse> ExecuteAsync(User user, string ip)
        {
            // 🟡 MFA ya generado pero no confirmado
            if (user.Mfa != null && user.Mfa.Enabled && !user.Mfa.Confirmed)
            {
                var qr = _mfaService.GenerateMfa(user);
                return new EnableMfaResponse(user.Mfa.Secret, qr.qrBase64);
            }
            // 🔒 No permitir doble MFA
            if (user.Mfa != null && user.Mfa.Enabled)
                throw new InvalidOperationException("MFA ya está habilitado");

            // 1️⃣ Generar secreto + QR
            var (secret, qrBase64) = _mfaService.GenerateMfa(user);

            // 2️⃣ Asociar MFA en estado PENDIENTE
            user.SetupMfa(secret);

            // 3️⃣ 🔥 PERSISTIR CAMBIOS (CRÍTICO)
            await _userRepository.UpdateAsync(user);

            // 4️⃣ Auditoría
            await _auditLogRepository.LogAsync(
                new AuditLog(user.Id, "EnableMfa", "MFA generado y pendiente de confirmación", ip)
            );

            // 5️⃣ Retornar QR + secret
            return new EnableMfaResponse(secret, qrBase64);
        }
    }
}
