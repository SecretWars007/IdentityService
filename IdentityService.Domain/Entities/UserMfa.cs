namespace IdentityService.Domain.Entities
{
    public class UserMfa
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public string Secret { get; private set; } = default!;
        public bool Enabled { get; private set; }
        public bool Confirmed { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public User User { get; private set; } = null!;

        public UserMfa() { }

        // 🔐 MFA generado (NO habilitado aún)
        public UserMfa(Guid userId, string secret)
        {
            UserId = userId;
            Secret = secret;
            Enabled = false; // ✅ CORRECTO
            Confirmed = false;
        }

        // ✅ Confirmación real del MFA
        public void Confirm()
        {
            if (Confirmed)
                throw new InvalidOperationException("MFA ya fue confirmado");

            Enabled = true;
            Confirmed = true;
        }

        public void Disable()
        {
            Enabled = false;
            Confirmed = false;
        }
    }
}
