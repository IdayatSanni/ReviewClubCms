using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReviewClubCms.Models;

namespace ReviewClubCms.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Reviewer> Reviewers { get; set; }
        public DbSet<ReviewerBook> ReviewerBooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            base.OnModelCreating(modelBuilder); 

            modelBuilder.Entity<ReviewerBook>()
                .HasKey(rb => new { rb.ReviewerId, rb.BookId }); 

            modelBuilder.Entity<ReviewerBook>()
                .HasOne(rb => rb.Reviewer)
                .WithMany(r => r.ReviewerBooks) 
                .HasForeignKey(rb => rb.ReviewerId);

            modelBuilder.Entity<ReviewerBook>()
                .HasOne(rb => rb.Book)
                .WithMany(b => b.ReviewerBooks) 
                .HasForeignKey(rb => rb.BookId);
        }
    }
}
