using System;

namespace UbuhlebethuConnectPro.Web.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
        public string? PaymentReference { get; set; }
    }
}
