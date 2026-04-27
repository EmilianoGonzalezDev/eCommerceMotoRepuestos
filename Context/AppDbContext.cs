using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Enums;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.EntityFrameworkCore;

namespace eCommerceMotoRepuestos.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Category> Category { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<Order> Order { get; set; }
    public DbSet<OrderItem> OrderItem { get; set; }
    public DbSet<CartItem> CartItem { get; set; }
    public DbSet<AppSetting> AppSetting { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(c =>
        {
            c.Property(x => x.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Product>(p =>
        {
            p.Property("Price").HasColumnType("decimal(10,2)");
            p.Property(x => x.IsActive).HasDefaultValue(true);
            p.HasOne(p => p.Category).WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);

        });

        modelBuilder.Entity<Order>(o =>
        {
            o.Property("TotalAmount").HasColumnType("decimal(10,2)");
            o.Property(x => x.Status).HasDefaultValue(OrderStatus.Pending);
            o.HasOne(o => o.User).WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

       modelBuilder.Entity<OrderItem>(oi =>
        {
            oi.Property("Price").HasColumnType("decimal(10,2)");
            oi.HasOne(oi => oi.Order).WithMany(o => o.OrderItems)
             .HasForeignKey(oi => oi.OrderId) 
             .OnDelete(DeleteBehavior.Restrict);
            oi.HasOne(oi => oi.Product).WithMany()
             .HasForeignKey(oi => oi.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CartItem>(ci =>
        {
            ci.HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
            ci.HasOne(ci => ci.User).WithMany()
              .HasForeignKey(ci => ci.UserId)
              .OnDelete(DeleteBehavior.Restrict);
            ci.HasOne(ci => ci.Product).WithMany()
              .HasForeignKey(ci => ci.ProductId)
              .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppSetting>(s =>
        {
            s.Property(x => x.Key).IsRequired();
            s.Property(x => x.Value).IsRequired();
            s.HasIndex(x => x.Key).IsUnique();
            s.HasData(
                new AppSetting
                {
                    AppSettingId = 1,
                    Key = AppSettingsKeys.LowStockThreshold,
                    Value = "5"
                }
            );
        });

        modelBuilder.Entity<User>(u =>
        {
            u.Property(u => u.Email).UseCollation("NOCASE");
            u.HasData(
                new User
                {
                    UserId = -1,
                    FullName = "Super Admin",
                    Email = "superadmin@admin",
                    Password = "AQAAAAIAAYagAAAAEFyeC2cws9pGHGRNbNLuJIEL0abroIXyMieFsiiI9K2yBi6CPSPI8FRiNwlW/yuu3Q==",
                    Type = "SuperAdmin"
                }
            );
        });

    }
}
