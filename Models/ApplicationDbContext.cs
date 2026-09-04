using Microsoft.EntityFrameworkCore;

namespace StokTakipUygulamasi.Models
{
    public class ApplicationDbContext : DbContext
    {
        // Kurucu metot (Veritabanı ayarlarını içeri alır)
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        // Veritabanında oluşacak tablolarımız (DbSet)
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}