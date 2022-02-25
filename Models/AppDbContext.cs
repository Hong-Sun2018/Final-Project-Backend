using Final_Project_Backend.Models.Classes;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_Backend.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }

        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(DBConfig.ConnString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasAlternateKey(user => user.UserName)
                .HasName("AK_UserName");

            modelBuilder.Entity<Category>()
                .HasAlternateKey(cate => cate.CategoryName)
                .HasName("AK_CategoryName");


        }

    }
    
}
