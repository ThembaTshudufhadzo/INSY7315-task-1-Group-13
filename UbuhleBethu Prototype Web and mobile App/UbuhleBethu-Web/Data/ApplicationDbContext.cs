using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace UbuhlebethuConnectPro.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Application entities
        public DbSet<UbuhlebethuConnectPro.Web.Models.QuoteRequest> QuoteRequests { get; set; }
        public DbSet<UbuhlebethuConnectPro.Web.Models.Notification> Notifications { get; set; }
        public DbSet<UbuhlebethuConnectPro.Web.Models.Invoice> Invoices { get; set; }
        public DbSet<UbuhlebethuConnectPro.Web.Models.Payment> Payments { get; set; }
        public DbSet<UbuhlebethuConnectPro.Web.Models.Project> Projects { get; set; }
        public DbSet<UbuhlebethuConnectPro.Web.Models.AuditLog> AuditLogs { get; set; }
    }
}
