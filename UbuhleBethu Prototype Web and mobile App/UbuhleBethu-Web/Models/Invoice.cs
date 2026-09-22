using System;

namespace UbuhlebethuConnectPro.Web.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public int QuoteRequestId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsPaid { get; set; } = false;
        public string? DocumentUrl { get; set; }
    }
}
