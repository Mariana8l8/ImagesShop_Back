using ImagesShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImagesShop.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Image> Images { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<ImageTag> ImageTags { get; set; } = null!;
        public DbSet<PurchaseHistory> Purchases { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImageTag>()
                .HasKey(imageTag => new { imageTag.ImageId, imageTag.TagId });

            modelBuilder.Entity<ImageTag>()
                .HasOne(imageTag => imageTag.Image)
                .WithMany(image => image.Tags)
                .HasForeignKey(imageTag => imageTag.ImageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ImageTag>()
                .HasOne(imageTag => imageTag.Tag)
                .WithMany()
                .HasForeignKey(imageTag => imageTag.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Order>()
                .HasOne(order => order.User)
                .WithMany(user => user.Orders)
                .HasForeignKey(order => order.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(orderItem => orderItem.Order)
                .WithMany(order => order.Items)
                .HasForeignKey(orderItem => orderItem.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(orderItem => orderItem.Image)
                .WithMany()
                .HasForeignKey(orderItem => orderItem.ImageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Image>()
                .Property(image => image.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(order => order.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PurchaseHistory>()
                .Property(purchaseHistory => purchaseHistory.ImagePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .Property(order => order.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<PurchaseHistory>()
                .Property(purchaseHistory => purchaseHistory.PurchasedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}