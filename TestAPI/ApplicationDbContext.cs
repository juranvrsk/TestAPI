using Microsoft.EntityFrameworkCore;
namespace TestAPI
{    
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) 
        { 

        }
        public DbSet<User> Users { get; set; }

        public DbSet<SignIn> SignIns { get; set; }

        public DbSet<Query> Queries { get; set; }
    }
}
