using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Application.DTOs
{
    public class EnableMfaResponse
    {
        public EnableMfaResponse(string secret, string qrBase64)
        {
            Secret = secret;
            QrBase64 = qrBase64;
        }

        public string Secret { get; set; } = null!;
        public string QrBase64 { get; set; } = null!;
    }
}
