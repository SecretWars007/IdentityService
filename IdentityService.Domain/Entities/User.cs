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

        public User() { } // EF Core

        public User(string fullName, string email, string passwordHash, DateTime birthDate)
        {
            FullName = fullName;
            Email = email;
            PasswordHash = passwordHash;
            BirthDate = birthDate;
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
    }
}
