using Microsoft.EntityFrameworkCore;

namespace StripeIntegrationSample.Models
{
    public class IntegrationDbContext : DbContext
    {
        public IntegrationDbContext() { }

        public IntegrationDbContext(DbContextOptions<IntegrationDbContext> options) : base(options) { }
        public virtual DbSet<StripePayment> StripePayments { get; set; }

    }
}
