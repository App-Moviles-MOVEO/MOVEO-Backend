using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Moveo_backend.Rental.Domain.Model.Aggregates;

namespace Moveo_backend.Rental.Infrastructure.Persistence.EFC.Configuration;

public class VehicleEntityTypeConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(v => v.Brand)
            .HasColumnName("brand")
            .IsRequired();

        builder.Property(v => v.Model)
            .HasColumnName("model")
            .IsRequired();

        builder.Property(v => v.Year)
            .HasColumnName("year")
            .IsRequired();

        builder.Property(v => v.Color)
            .HasColumnName("color");

        builder.Property(v => v.Transmission)
            .HasColumnName("transmission");

        builder.Property(v => v.FuelType)
            .HasColumnName("fuel_type");

        builder.Property(v => v.Seats)
            .HasColumnName("seats");

        builder.Property(v => v.DailyPrice)
            .HasColumnName("daily_price")
            .IsRequired();

        builder.Property(v => v.DepositAmount)
            .HasColumnName("deposit_amount");

        builder.Property(v => v.Location)
            .HasColumnName("location");

        builder.Property(v => v.Status)
            .HasColumnName("status")
            .HasDefaultValue("available");
    }
}