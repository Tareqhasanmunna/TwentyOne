using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.DAL.Entities;

namespace TwentyOne.DAL.Context
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        // DbSet properties for your entities go here
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<PreOrder> PreOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Fluent API configurations go here

            // Brand → Product (one to many)
            builder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product → ProductImage (one to many)
            builder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal columns for discounts
            builder.Entity<Product>()
                .Property(p => p.DiscountPercentage)
                .HasColumnType("decimal(5,2)");

            // Discount amount should also be decimal(18,2) to match price columns
            builder.Entity<Product>()
                .Property(p => p.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            // Order → OrderItem (one to many)
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product → OrderItem (one to many)
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // User → Order (one to many)
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User → WishlistItem (one to many)
            builder.Entity<WishlistItem>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product → WishlistItem (one to many)
            builder.Entity<WishlistItem>()
                .HasOne(w => w.Product)
                .WithMany(p => p.WishlistItems)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → Review (one to many)
            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product → Review (one to many)
            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Price columns — always use decimal(18,2) for money
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.FinalAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<OrderItem>()
                .Property(oi => oi.Subtotal)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Coupon>()
                .Property(c => c.DiscountValue)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Coupon>()
                .Property(c => c.MinimumOrderAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Order>()
                .Property(o => o.DeliveryCharge)
                .HasColumnType("decimal(18,2)");

            // User → PreOrder
            builder.Entity<PreOrder>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product → PreOrder
            builder.Entity<PreOrder>()
                .HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal columns
            builder.Entity<PreOrder>()
                .Property(p => p.DepositAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<PreOrder>()
                .Property(p => p.ProductPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<PreOrder>()
                .Property(p => p.RemainingAmount)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Product>()
                .Property(p => p.PreOrderDeposit)
                .HasColumnType("decimal(18,2)");
        }
    }
}
