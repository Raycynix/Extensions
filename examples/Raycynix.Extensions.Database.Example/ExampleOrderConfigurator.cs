using Microsoft.EntityFrameworkCore;
using Raycynix.Extensions.Database.Implementations;

namespace Raycynix.Extensions.Database.Example;

internal sealed class ExampleOrderConfigurator : GenericConfigurator<ExampleOrder>
{
    public override Type[] DependsOn => [];

    public override void Configure(ModelBuilder modelBuilder)
    {
        var entity = ConfigureEntity(modelBuilder, "example_orders");

        entity.HasKey(current => current.Id);
        entity.HasIndex(current => current.Number).IsUnique();

        entity.Property(current => current.Number).HasMaxLength(64).IsRequired();
        entity.Property(current => current.CustomerName).HasMaxLength(200).IsRequired();
        entity.Property(current => current.Status).HasMaxLength(32).IsRequired();
        entity.Property(current => current.TotalAmount).HasPrecision(18, 2);
    }

    public override void Seed(ModelBuilder modelBuilder)
    {
        ConfigureEntity(modelBuilder, "example_orders").HasData(new ExampleOrder
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Number = "ORD-2026-0001",
            CustomerName = "Seed Customer",
            TotalAmount = 149.90m,
            Status = "Completed",
            CreatedAt = new DateTime(2026, 04, 14, 12, 00, 00)
        });
    }
}
