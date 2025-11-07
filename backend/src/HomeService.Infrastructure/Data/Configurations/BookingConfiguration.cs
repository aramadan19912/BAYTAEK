using HomeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeService.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.TotalAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.VatAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.VatPercentage)
            .HasPrecision(5, 2);

        builder.Property(x => x.Currency)
            .HasConversion<string>();

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Service)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Provider)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Address)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Payment)
            .WithOne(x => x.Booking)
            .HasForeignKey<Payment>(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Review)
            .WithOne(x => x.Booking)
            .HasForeignKey<Review>(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ScheduledAt);
        builder.HasIndex(x => x.Status);
    }
}
