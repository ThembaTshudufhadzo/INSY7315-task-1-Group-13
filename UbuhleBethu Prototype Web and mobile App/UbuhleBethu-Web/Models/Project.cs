using System;

namespace UbuhlebethuConnectPro.Web.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string QuoteReference { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Active";
        public decimal EstimatedCost { get; set; }
    }
}
