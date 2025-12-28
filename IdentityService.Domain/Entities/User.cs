using System;
using System.Collections.Generic;
using System.Linq;
using IdentityService.Domain.Entities.IdentityService.Domain.Entities;

namespace IdentityService.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Datos principales
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTime BirthDate { get; set; }

        // Roles (N–N)
        public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();

        // Datos personales
        public UserProfile? Profile { get; set; }

        // MFA (agregado)
        private UserMfa? _mfa;
        public UserMfa? Mfa => _mfa;
        public bool IsMfaEnabled => _mfa?.Enabled == true;

        // failed login attempts
        public int FailedLoginAttempts { get; private set; }
        public int LockoutCount { get; private set; }
        public DateTime? LockoutEnd { get; private set; }
        public bool IsDisabled { get; private set; }
        public int TokenVersion { get; private set; } = 1;

        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;

        // ==== CONSTRUCTORES ====
        public User() { } // EF Core

        public User(
            string fullName,
            string email,
            string passwordHash,
            DateTime birthDate,
            int tokenVersion
        )
        {
            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
            BirthDate = birthDate;
            TokenVersion = tokenVersion;
        }

        // ==== MÉTODOS DE DOMINIO ====

        // Roles
        public void AssignRole(Guid roleId)
        {
            if (Roles.Any(r => r.RoleId == roleId))
                return;

            Roles.Add(new UserRole(Id, roleId));
        }

        // Perfil
        public void SetProfile(
            string documentNumber,
            string address,
            string phoneNumber,
            string? photoUrl
        )
        {
            Profile = new UserProfile(Id, documentNumber, address, phoneNumber, photoUrl);
        }

        // ==== MFA ====
        public void SetupMfa(string secret)
        {
            if (_mfa != null)
                throw new InvalidOperationException("MFA ya configurado");

            _mfa = new UserMfa(Id, secret);
        }

        public void ConfirmMfa()
        {
            if (_mfa == null)
                throw new InvalidOperationException("MFA no inicializado");

            _mfa.Confirm();
        }

        public void DisableMfa()
        {
            _mfa?.Disable();
        }

        public void RegisterFailedLogin(SystemSettings settings)
        {
            FailedLoginAttempts++;

            if (FailedLoginAttempts >= settings.MaxFailedLoginAttempts)
            {
                LockoutCount++;
                LockoutEnd = DateTime.UtcNow.Add(settings.LockoutDuration);
                FailedLoginAttempts = 0;

                if (LockoutCount >= settings.MaxLockouts)
                {
                    IsDisabled = true;
                }
            }
        }

        public void RegisterSuccessfulLogin()
        {
            FailedLoginAttempts = 0;
            LockoutEnd = null;
        }

        public void InvalidateTokens()
        {
            TokenVersion++;
        }

        public void DisableAccount()
        {
            IsDisabled = true;
            InvalidateTokens();
        }

        public void RegisterFailedLogin()
        {
            FailedLoginAttempts++;
        }

        public void ResetFailedLogins()
        {
            FailedLoginAttempts = 0;
        }

        public void LockUntil(DateTime until)
        {
            LockoutEnd = until;
            LockoutCount++;
        }

        public void IncrementTokenVersion()
        {
            TokenVersion++;
        }
    }
}
