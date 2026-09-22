namespace UbuhlebethuConnectPro.Web.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string JobNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BuildingType { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Progress { get; set; }
        public int Variance { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal InvoicedAmount { get; set; }
        public string Status { get; set; } = "In Progress";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string SiteManager { get; set; } = string.Empty;
        public bool NhbrcCompliant { get; set; } = true;
    }
}