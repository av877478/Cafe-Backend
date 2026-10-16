using Cafe.Model.Model;
using Microsoft.EntityFrameworkCore;

namespace Cafe.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RegistrationChallenge> RegistrationChallenges { get; set; } = null!;
        public DbSet<CafeTable> Tables { get; set; } = null!;
        public DbSet<TableQrCode> TableQrCodes { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<StaffMember> StaffMembers { get; set; } = null!;
        public DbSet<CafeSetting> Settings { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MobileNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50).HasDefaultValue("Admin");
                entity.Property(e => e.AccountStatus).IsRequired().HasMaxLength(50).HasDefaultValue("ACTIVE");
            });

            modelBuilder.Entity<RegistrationChallenge>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ChallengeId).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.Property(e => e.ChallengeId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Otp).IsRequired().HasMaxLength(10);
            });

            modelBuilder.Entity<CafeTable>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Number).IsUnique();
                entity.Property(e => e.Number).IsRequired();
                entity.Property(e => e.Capacity).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(30).HasDefaultValue("Available");
            });

            modelBuilder.Entity<TableQrCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TableId);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.Property(e => e.Token).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.ItemCount).HasDefaultValue(0);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CategoryId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(30).HasDefaultValue("Active");
                entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.IsFeatured).HasDefaultValue(false);
                entity.Property(e => e.IsPopular).HasDefaultValue(false);
                entity.Property(e => e.PreparationTime).HasDefaultValue(5);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.OrderTime);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(50).HasDefaultValue("Dine-In");
                entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("New");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Tax).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");

                entity.HasMany(e => e.Items)
                      .WithOne(i => i.Order)
                      .HasForeignKey(i => i.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderId);
                entity.Property(e => e.ProductId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<StaffMember>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50).HasDefaultValue("Waiter");
                entity.Property(e => e.Phone).HasMaxLength(30);
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<CafeSetting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CafeName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.LogoUrl).HasMaxLength(1000);
                entity.Property(e => e.Address).HasMaxLength(300);
                entity.Property(e => e.Phone).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.FestivalTheme).IsRequired().HasMaxLength(50).HasDefaultValue("normal");
                entity.Property(e => e.TaxRate).HasColumnType("decimal(18,2)").HasDefaultValue(8.0m);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(10).HasDefaultValue("USD");
            });
        }
    }
}
