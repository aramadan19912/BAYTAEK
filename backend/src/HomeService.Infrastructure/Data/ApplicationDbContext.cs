using HomeService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HomeService.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ServiceProvider> ServiceProviders => Set<ServiceProvider>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ProviderAvailability> ProviderAvailabilities => Set<ProviderAvailability>();
    public DbSet<ProviderBlockedDate> ProviderBlockedDates => Set<ProviderBlockedDate>();
    public DbSet<Payout> Payouts => Set<Payout>();
    public DbSet<PayoutBooking> PayoutBookings => Set<PayoutBooking>();
    public DbSet<Dispute> Disputes => Set<Dispute>();
    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();
    public DbSet<BookingHistory> BookingHistories => Set<BookingHistory>();
    // public DbSet<NotificationSetting> NotificationSettings => Set<NotificationSetting>(); // Entity doesn't exist

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Configure Report entity relationships (multiple FKs to User)
        modelBuilder.Entity<Report>()
            .HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Report>()
            .HasOne(r => r.ReportedUser)
            .WithMany()
            .HasForeignKey(r => r.ReportedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Report>()
            .HasOne(r => r.ReviewedBy)
            .WithMany()
            .HasForeignKey(r => r.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Dispute entity relationships (multiple FKs to User)
        modelBuilder.Entity<Dispute>()
            .HasOne(d => d.RaisedByUser)
            .WithMany()
            .HasForeignKey(d => d.RaisedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Dispute>()
            .HasOne(d => d.AssignedToUser)
            .WithMany()
            .HasForeignKey(d => d.AssignedTo)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure SupportTicket entity relationships (multiple FKs to User)
        modelBuilder.Entity<SupportTicket>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SupportTicket>()
            .HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SupportTicket>()
            .HasOne(t => t.ResolvedBy)
            .WithMany()
            .HasForeignKey(t => t.ResolvedById)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Domain.Common.BaseEntity entity)
            {
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
