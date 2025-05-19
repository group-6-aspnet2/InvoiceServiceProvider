using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Contexts;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<InvoiceEntity> Invoices { get; set; } = null!;
    public DbSet<InvoiceItemEntity> InvoiceItems { get; set; } = null!;
    public DbSet<InvoiceStatusEntity> InvoiceStatuses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InvoiceStatusEntity>().HasData(
            new InvoiceStatusEntity { Id = 1, StatusName = "Unpaid" },
            new InvoiceStatusEntity { Id = 2, StatusName = "Paid" },
            new InvoiceStatusEntity { Id = 3, StatusName = "Held" },
            new InvoiceStatusEntity { Id = 4, StatusName = "Overdue" }
        );

        modelBuilder.Entity<InvoiceItemEntity>(builder =>
        {
            builder.HasKey(i => i.Id);

            builder
                .HasOne(i => i.Invoice)
                .WithMany(inv => inv.InvoiceItems)
                .HasForeignKey(i => i.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
