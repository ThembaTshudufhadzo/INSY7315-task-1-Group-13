namespace UbuhlebethuConnectPro.Web.Models
{
    public class QuoteRequest
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ProjectType { get; set; } = string.Empty; // e.g., RDP, Social Housing, Civil Works
        public string SiteLocation { get; set; } = string.Empty;
        public decimal EstimatedBudget { get; set; }
        public string ProjectDescription { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; } = DateTime.Now;
        // Workflow fields
        public string Status { get; set; } = "Draft"; // Draft, Submitted, Reviewed, Approved, Invoiced, Paid
        public string Reference { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty; // Identity username
        public bool IsPaid { get; set; } = false;
        public int? InvoiceId { get; set; }
    }
}