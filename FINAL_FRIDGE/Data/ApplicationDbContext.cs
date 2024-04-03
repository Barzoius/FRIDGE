using FINAL_FRIDGE.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FINAL_FRIDGE.Models;
using System.ComponentModel.DataAnnotations.Schema;


namespace FINAL_FRIDGE.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public DbSet<Like> Likes { get; set; }

        //public DbSet<Profile> Profiles { get; set; }
        public DbSet<BookmarkCategory> BookmarkCategories { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // definirea relatiei many-to-many dintre Bookmark si Category

            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<BookmarkCategory>()
                .HasKey(bc => new { bc.Id, bc.BookmarkId, bc.CategoryId });


            // definire relatii cu modelele Bookmark si Category
            //Foreign Key

            modelBuilder.Entity<BookmarkCategory>()
                .HasOne(bc => bc.Bookmark)
                .WithMany(bc => bc.BookmarkCategories)
                .HasForeignKey(bc => bc.BookmarkId);

            modelBuilder.Entity<BookmarkCategory>()
                .HasOne(bc => bc.Category)
                .WithMany(bc => bc.BookmarkCategories)
                .HasForeignKey(bc => bc.CategoryId);
        }
    }
}