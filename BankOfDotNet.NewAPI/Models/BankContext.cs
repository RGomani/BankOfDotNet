using Microsoft.EntityFrameworkCore;

namespace BankOfDotNet.NewAPI.Models
{
    /// <summary>
    /// This is the DB Context
    /// </summary>
    public class BankContext : DbContext
    {
        public BankContext(DbContextOptions<BankContext> options)
            : base(options) { }

        // The set of customers 
        public DbSet<Customer> Customers { get; set; }
    }
}
