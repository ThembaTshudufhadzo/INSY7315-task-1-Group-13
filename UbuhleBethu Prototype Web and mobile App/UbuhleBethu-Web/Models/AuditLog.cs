using System;

namespace UbuhlebethuConnectPro.Web.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string ActionType { get; set; } = string.Empty; // e.g., DeactivateUser, ReactivateUser, EditRoles
        public string PerformedBy { get; set; } = string.Empty; // username who performed action
        public string TargetUserId { get; set; } = string.Empty; // affected user id
        public string? Reason { get; set; }
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
