using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Application.DTOs
{
    public class ConfirmMfaRequest
    {
        public string Code { get; set; } = null!;
    }
}
