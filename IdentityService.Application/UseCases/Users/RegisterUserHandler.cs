using System;
using System.Security.Cryptography;
using System.Text;
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

        public RegisterUserHandler(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task ExecuteAsync(RegisterUserUseCase useCase)
        {
            var req = useCase.Request;
            // -----------------------------
            // 1️⃣ Validar que el correo no exista
            // -----------------------------
            if (await _userRepository.GetByEmailAsync(req.Email) != null)
                throw new InvalidOperationException("El correo ya está registrado");
            // -----------------------------
            // 2️⃣ Validar rol
            // -----------------------------
            var role =
                await _roleRepository.GetByIdAsync(req.RoleId)
                ?? throw new InvalidOperationException("Rol inválido");
            // -----------------------------
            // 3️⃣ Validar contraseña fuerte (mínimo 8, mayúscula, minúscula, número y símbolo)
            // -----------------------------
            if (!IsStrongPassword(req.Password))
                throw new InvalidOperationException(
                    "La contraseña no cumple los requisitos de seguridad"
                );
            var passwordHash = HashPassword(req.Password);

            var user = new User(req.FullName, req.Email, passwordHash, req.BirthDate);
            user.SetProfile(req.DocumentNumber, req.Address, req.Phone, req.PhotoUrl);
            user.AssignRole(role.Id);

            await _userRepository.AddAsync(user);
        }

        // -----------------------------
        // Método para hash de contraseña (Se recomienda usar BCrypt)
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
            if (password.Length < 8)
                return false;
            if (!HasUppercase(password))
                return false;
            if (!HasLowercase(password))
                return false;
            if (!HasDigit(password))
                return false;
            if (!HasSpecialChar(password))
                return false;
            return true;
        }

        private bool HasUppercase(string s) => Regex.IsMatch(s, "[A-Z]");

        private bool HasLowercase(string s) => Regex.IsMatch(s, "[a-z]");

        private bool HasDigit(string s) => Regex.IsMatch(s, "[0-9]");

        private bool HasSpecialChar(string s) => Regex.IsMatch(s, "[^a-zA-Z0-9]");
    }
}
