using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;

namespace IdentityService.Application.UseCases.Users
{
    public class RegisterUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAuditLogRepository _auditLogRepository;

        public RegisterUserHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IAuditLogRepository auditLogRepository
        )
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _auditLogRepository = auditLogRepository;
        }

        public async Task<User> ExecuteAsync(RegisterUserUseCase useCase)
        {
            var req = useCase.Request;
            var email = req.Email.ToLower();
            Guid? newUserId = null;

            try
            {
                // -----------------------------
                // 1️⃣ Validar que el correo no exista
                // -----------------------------
                if (await _userRepository.GetByEmailAsync(email) != null)
                    throw new InvalidOperationException("El correo ya está registrado");

                // -----------------------------
                // 2️⃣ Validar rol
                // -----------------------------
                var role = await _roleRepository.GetByIdAsync(req.RoleId);
                if (role == null)
                    throw new InvalidOperationException("Rol inválido");

                // -----------------------------
                // 3️⃣ Validar contraseña fuerte
                // -----------------------------
                if (!IsStrongPassword(req.Password))
                    throw new InvalidOperationException(
                        "La contraseña no cumple los requisitos de seguridad"
                    );

                var passwordHash = HashPassword(req.Password);

                // -----------------------------
                // 4️⃣ Crear usuario y asignar perfil
                // -----------------------------
                var user = new User(
                    req.FullName,
                    email,
                    passwordHash,
                    req.BirthDate,
                    tokenVersion: 1
                );

                user.SetProfile(req.DocumentNumber, req.Address, req.Phone, req.PhotoUrl);

                // -----------------------------
                // 5️⃣ Asignar rol
                // -----------------------------
                user.AssignRole(role.Id);

                // -----------------------------
                // 6️⃣ Guardar en BD
                // -----------------------------
                await _userRepository.AddAsync(user);
                newUserId = user.Id;

                // -----------------------------
                // 7️⃣ Audit log: éxito
                // -----------------------------
                await _auditLogRepository.LogAsync(
                    new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        Action = "UserRegistration",
                        Result = "Success",
                        Timestamp = DateTime.UtcNow,
                        UserId = newUserId,
                        IpAddress = string.Empty,
                    }
                );

                return user;
            }
            catch (Exception ex)
            {
                // -----------------------------
                // 8️⃣ Audit log: fallo
                // -----------------------------
                await _auditLogRepository.LogAsync(
                    new AuditLog
                    {
                        Id = Guid.NewGuid(),
                        Action = "UserRegistration",
                        Result = $"Failed: {ex.Message}",
                        Timestamp = DateTime.UtcNow,
                        UserId = newUserId, // null si no se creó usuario
                        IpAddress = string.Empty,
                    }
                );

                throw; // relanza excepción para que el controlador maneje
            }
        }

        // -----------------------------
        // Método para hash de contraseña
        // -----------------------------
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // -----------------------------
        // Método para validar contraseña fuerte
        // -----------------------------
        private bool IsStrongPassword(string password)
        {
            return password.Length >= 8
                && HasUppercase(password)
                && HasLowercase(password)
                && HasDigit(password)
                && HasSpecialChar(password);
        }

        private bool HasUppercase(string s) => Regex.IsMatch(s, "[A-Z]");

        private bool HasLowercase(string s) => Regex.IsMatch(s, "[a-z]");

        private bool HasDigit(string s) => Regex.IsMatch(s, "[0-9]");

        private bool HasSpecialChar(string s) => Regex.IsMatch(s, "[^a-zA-Z0-9]");
    }
}
