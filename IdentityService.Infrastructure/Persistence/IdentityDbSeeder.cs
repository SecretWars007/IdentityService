using System.Security.Cryptography;
using System.Text;
using IdentityService.Domain.Entities;
using IdentityService.Domain.Entities.IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence
{
    public static class IdentityDbSeeder
    {
        public static async Task SeedAsync(IdentityDbContext context)
        {
            // =========================
            // 1️⃣ Roles
            // =========================
            if (!context.Roles.Any())
            {
                var adminRole = new Role { Name = "ADMINISTRADOR" };
                var supervisorRole = new Role { Name = "SUPERVISOR" };
                var userRole = new Role { Name = "VENDEDOR" };
                var queryRole = new Role { Name = "CONSULTA" };

                await context.Roles.AddRangeAsync(adminRole, supervisorRole, userRole, queryRole);
                await context.SaveChangesAsync();
            }

            // =========================
            // 2️⃣ Usuario ADMIN
            // =========================
            if (!context.Users.Any(u => u.Email == "admin@system.com"))
            {
                var password = "Admin123!"; // Cambiar si quieres
                var hashedPassword = HashPassword(password); // Ahora con BCrypt

                var adminUser = new User
                {
                    FullName = "Administrador Principal",
                    Email = "admin@system.com",
                    PasswordHash = hashedPassword,
                    BirthDate = DateTime.UtcNow.AddYears(-30), // ejemplo
                };

                // Perfil básico
                adminUser.Profile = new UserProfile
                {
                    UserId = adminUser.Id,
                    DocumentNumber = "00000000",
                    Address = "Sede Principal",
                    Phone = "+59100000000",
                    PhotoUrl = "https://i.pravatar.cc/150?img=1",
                };

                // Obtener rol ADMIN
                var adminRole = await context.Roles.FirstAsync(r => r.Name == "ADMINISTRADOR");

                adminUser.Roles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }

        // =========================
        // Método para hashear contraseña con BCrypt
        // =========================
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
