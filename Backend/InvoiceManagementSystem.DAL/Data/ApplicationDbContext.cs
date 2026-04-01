using Microsoft.EntityFrameworkCore;
using InvoiceManagementSystem.DAL.Entities;

namespace InvoiceManagementSystem.DAL.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<PaymentMethod> PaymentMethods { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<User> Users { get; set; }

       protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Primary Keys
    modelBuilder.Entity<Invoice>().HasKey(i => i.InvoiceId);
    modelBuilder.Entity<InvoiceLineItem>().HasKey(li => li.LineItemId);
    modelBuilder.Entity<Payment>().HasKey(p => p.PaymentId);
    modelBuilder.Entity<PaymentMethod>().HasKey(pm => pm.MethodId);


    modelBuilder.Entity<Invoice>()
    .HasOne(i => i.Customer)
    .WithMany(c => c.Invoices)
    .HasForeignKey(i => i.CustomerId)
    .OnDelete(DeleteBehavior.Restrict);

    // Configure Decimal Precision
modelBuilder.Entity<Invoice>()
    .Property(i => i.SubTotal)
    .HasPrecision(18, 2);

modelBuilder.Entity<Invoice>()
    .Property(i => i.Tax)
    .HasPrecision(18, 2);

modelBuilder.Entity<Invoice>()
    .Property(i => i.Discount)
    .HasPrecision(18, 2);

modelBuilder.Entity<Invoice>()
    .Property(i => i.GrandTotal)
    .HasPrecision(18, 2);

modelBuilder.Entity<InvoiceLineItem>()
    .Property(li => li.UnitPrice)
    .HasPrecision(18, 2);

modelBuilder.Entity<InvoiceLineItem>()
    .Property(li => li.Tax)
    .HasPrecision(18, 2);

modelBuilder.Entity<InvoiceLineItem>()
    .Property(li => li.Discount)
    .HasPrecision(18, 2);

modelBuilder.Entity<InvoiceLineItem>()
    .Property(li => li.LineTotal)
    .HasPrecision(18, 2);

modelBuilder.Entity<Payment>()
    .Property(p => p.PaymentAmount)
    .HasPrecision(18, 2);

    // Invoice → LineItems (One-to-Many)
    modelBuilder.Entity<Invoice>()
        .HasMany(i => i.LineItems)
        .WithOne(li => li.Invoice)
        .HasForeignKey(li => li.InvoiceId)
        .OnDelete(DeleteBehavior.Cascade);

    // Invoice → Payments (One-to-Many)
    modelBuilder.Entity<Invoice>()
        .HasMany(i => i.Payments)
        .WithOne(p => p.Invoice)
        .HasForeignKey(p => p.InvoiceId)
        .OnDelete(DeleteBehavior.Cascade);

    // Unique Invoice Number
    modelBuilder.Entity<Invoice>()
        .HasIndex(i => i.InvoiceNumber)
        .IsUnique();
}
    }
}