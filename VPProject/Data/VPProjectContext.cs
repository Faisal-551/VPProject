using Microsoft.EntityFrameworkCore;
using VPProject.Models;

namespace VPProject.Data
{
    public class VPProjectContext : DbContext
    {
        public VPProjectContext(DbContextOptions<VPProjectContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Category { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Cart> Cart { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed default admin account
            modelBuilder.Entity<Admin>().HasData(
                new Admin
                {
                    AdminId = 1,
                    Username = "admin",
                    Password = "admin123",
                    FullName = "System Administrator",
                    Email = "admin@vpproject.com",
                    Role = "Manager"
                }
            );
        }
    }
}