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
            builder.Entity<User>()
                .HasMany(u => u.Tests)
                .WithOne(t => t.Creator)
                .IsRequired()
                .HasForeignKey(t => t.CreatorId);

            builder.Entity<Rate>()
                .HasOne(r => r.Test)
                .WithMany(t => t.Rates)
                .IsRequired()
                .HasForeignKey(r => r.TestId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Rate>()
                .HasOne(r => r.User)
                .WithMany(u => u.Rates)
                .IsRequired()
                .HasForeignKey(r=> r.UserId);

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