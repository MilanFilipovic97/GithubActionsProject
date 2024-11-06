using IdentityApi.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityApi.Database
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Company> Companies { get; set; }
    }
}
