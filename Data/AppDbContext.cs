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
            //builder.Entity<Answer>()
            //    .HasOne(a => a.Question)
            //    .WithMany(q => q.MultipleAnswers)
            //    .IsRequired()
            //    .HasForeignKey(a => a.QuestionId);

            //builder.Entity<Test>()
            //    .Navigation(t => t.Questions).AutoInclude();

            //builder.Entity<Test>()
            //    .Navigation(t => t.Creator).AutoInclude();

            //builder.Entity<Question>()
            //    .Navigation(q => q.MultipleAnswers).AutoInclude();

            //builder.Entity<Question>()
            //    .Navigation(q=>q.Test).AutoInclude();

            //builder.Entity<Answer>()
            //    .Navigation(a => a.Question).AutoInclude();
            
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
    }
}