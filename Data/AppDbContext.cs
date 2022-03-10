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
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Rate>()
                .HasOne(r => r.Test)
                .WithOne(t => t.Rates)
                .IsRequired()
                .HasForeignKey(r => r.TestId);
            
            base.OnModelCreating(builder);
        }
        public AppDbContext(DbContextOptions options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Rate> Rates { get; set; }
    }
}