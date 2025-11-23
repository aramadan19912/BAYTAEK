using HomeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeService.Infrastructure.Data.Configurations;

public class BookingHistoryConfiguration : IEntityTypeConfiguration<BookingHistory>
{
    public void Configure(EntityTypeBuilder<BookingHistory> builder)
    {
        // Configure relationships with NO ACTION on delete to prevent cascade cycles
        builder.HasOne(bh => bh.Booking)
            .WithMany()
            .HasForeignKey(bh => bh.BookingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(bh => bh.ChangedBy)
            .WithMany()
            .HasForeignKey(bh => bh.ChangedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
