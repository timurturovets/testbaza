using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;


using TestBaza.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace TestBaza.Data
{
#pragma warning disable 8618
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
    }
}