using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using OtpNet;
using QRCoder;

namespace IdentityService.Infrastructure.Auth
{
    public class MfaService : IMfaService
    {
        public (string secret, string qrBase64) GenerateMfa(User user)
        {
            var secretKey = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));
            var uri = $"otpauth://totp/Inventory:{user.Email}?secret={secretKey}&issuer=Inventory";

            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new Base64QRCode(qrData);
            var qrBase64 = qrCode.GetGraphic(10);

            return (secretKey, qrBase64);
        }

        public bool ValidateMfa(string secret, string code)
        {
            var bytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(bytes);
            return totp.VerifyTotp(code, out _, new VerificationWindow(1, 1));
        }
    }
}
