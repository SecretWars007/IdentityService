using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;

        public AuditLog() { }

        public AuditLog(Guid? userId, string action, string result, string ip)
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            UserId = userId;
            Action = action;
            Result = result;
            IpAddress = ip;
        }
    }
}
