using Microsoft.EntityFrameworkCore;
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

            builder.Entity<User>()
                .HasMany(u => u.CheckInfos)
                .WithOne(i => i.Checker)
                .IsRequired()
                .HasForeignKey(i => i.CheckerId);

            builder.Entity<CheckInfo>()
                .HasOne(i => i.Attempt)
                .WithOne(a => a.CheckInfo)
                .HasForeignKey<CheckInfo>(i => i.AttemptId);

            builder.Entity<Attempt>()
                .Navigation(a => a.CheckInfo).AutoInclude();

            builder.Entity<Rate>()
                .HasOne(r => r.Test)
                .WithMany(t => t.Rates)
                .IsRequired()
                .HasForeignKey(r => r.TestId);

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
        public DbSet<PassingInfo> PassingInfos { get; set; }
        public DbSet<CheckInfo> CheckInfos { get; set; }
        public DbSet<Attempt> Attempts { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
    }
}