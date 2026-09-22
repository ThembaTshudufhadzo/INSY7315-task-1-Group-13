using System;

namespace UbuhlebethuConnectPro.Web.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty; // Identity username
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public string? ReferenceId { get; set; }
    }
}
