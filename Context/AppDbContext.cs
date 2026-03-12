using eCommerceMotoRepuestos.Entities;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(c =>
        {
            //c.HasKey("CategoryId"); //no es necesario, EF lo asume automaticamente
            //c.Property("CategoryId").ValueGeneratedOnAdd(); //no es necesario, EF lo asume automaticamente
            c.Property(x => x.IsActive).HasDefaultValue(true);
            c.HasData(
                new Category { CategoryId = 1, Name = "Technology", IsActive = true },
                new Category { CategoryId = 2, Name = "Bedroom", IsActive = true }
            );
        });

        modelBuilder.Entity<Product>(p =>
        {
            p.Property("Price").HasColumnType("decimal(10,2)");
            p.Property(x => x.IsActive).HasDefaultValue(true);
            p.HasOne(p => p.Category).WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.Restrict); //para evitar eliminar los productos de una categoria al eliminar dicha categoria

        });

        modelBuilder.Entity<Order>(o =>
        {
            o.Property("TotalAmount").HasColumnType("decimal(10,2)");
            o.HasOne(o => o.User).WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

       modelBuilder.Entity<OrderItem>(oi =>
        {
            oi.Property("Price").HasColumnType("decimal(10,2)");
            oi.HasOne(oi => oi.Order).WithMany(o => o.OrderItems)
             .HasForeignKey(oi => oi.OrderId) //para mi era .HasForeignKey(oi => oi.OrderItemId)
             .OnDelete(DeleteBehavior.Restrict);
            oi.HasOne(oi => oi.Product).WithMany()
             .HasForeignKey(oi => oi.ProductId) //para mi era .HasForeignKey(oi => oi.OrderItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });


    }
}
