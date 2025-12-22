using Microsoft.EntityFrameworkCore;
using EvChargingSystem.API.Models;

namespace EvChargingSystem.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet Properties: Кожна з них представляє таблицю в БД
        public DbSet<User> Users { get; set; }
        public DbSet<Site> Sites { get; set; }
        public DbSet<ChargingPoint> ChargingPoints { get; set; }
        public DbSet<ElectricityRate> ElectricityRates { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Налаштування зв'язку 1:N між Users та Sites (для Адміністратора Площадки)
            modelBuilder.Entity<Site>()
                .HasOne(s => s.Administrator)        // Сайт має одного Адміністратора
                .WithMany(u => u.ManagedSites)       // Користувач може керувати багатьма Сайтами
                .HasForeignKey(s => s.AdministratorId)
                .OnDelete(DeleteBehavior.Restrict);  // Запобігаємо видаленню Користувача, який є Адміністратором
        }
    }
}