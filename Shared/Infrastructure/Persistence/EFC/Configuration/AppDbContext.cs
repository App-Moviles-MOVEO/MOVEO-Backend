using EntityFrameworkCore.CreatedUpdatedDate.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Moveo_backend.Adventure.Domain.Model.Aggregate;
using Moveo_backend.Chat.Domain.Model.Aggregate;
using Moveo_backend.Notification.Domain.Model.Aggregate;
using Moveo_backend.Rental.Domain.Model.Aggregates;
using Moveo_backend.Rental.Domain.Model.ValueObjects;
using Moveo_backend.Support.Domain.Model.Aggregate;
using Moveo_backend.UserManagement.Domain.Model.Aggregates;
using Moveo_backend.Payment.Domain.Model.Aggregate;
using PaymentEntity = Moveo_backend.Payment.Domain.Model.Aggregate.Payment;
using NotificationEntity = Moveo_backend.Notification.Domain.Model.Aggregate.Notification;
using UserReviewEntity = Moveo_backend.UserReview.Domain.Model.Aggregate.UserReview;

namespace Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<Rental.Domain.Model.Aggregates.Rental> Rentals { get; set; } = null!;
    public DbSet<RentalInspection> RentalInspections { get; set; } = null!;
    public DbSet<AdventureRoute> AdventureRoutes { get; set; } = null!;
    public DbSet<RoutePassenger> RoutePassengers { get; set; } = null!;
    public DbSet<PaymentEntity> Payments { get; set; } = null!;
    public DbSet<Withdrawal> Withdrawals { get; set; } = null!;
    public DbSet<NotificationEntity> Notifications { get; set; } = null!;
    public DbSet<DeviceToken> DeviceTokens { get; set; } = null!;
    public DbSet<SupportTicket> SupportTickets { get; set; } = null!;
    public DbSet<TicketMessage> TicketMessages { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<UserReviewEntity> UserReviews { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddCreatedUpdatedInterceptor();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // -------------------- USER --------------------
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FirstName).IsRequired();
            entity.Property(u => u.LastName).IsRequired();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).IsRequired();
            entity.Property(u => u.Phone);
            entity.Property(u => u.Dni);
            entity.Property(u => u.LicenseNumber);
            entity.Property(u => u.Address);
            entity.Property(u => u.PreferredLanguage);
            entity.Property(u => u.EmailNotifications);
            entity.Property(u => u.PushNotifications);
            entity.Property(u => u.SmsNotifications);
            entity.Property(u => u.AutoAcceptRentals);
            entity.Property(u => u.MinimumRentalDays);
            entity.Property(u => u.InstantBooking);
            entity.Property(u => u.CreatedAt);
            entity.Property(u => u.UpdatedAt);
            
            // Ignorar propiedades computadas
            entity.Ignore(u => u.Name);
            entity.Ignore(u => u.Preferences);
            entity.Ignore(u => u.FullName);
            entity.Ignore(u => u.EmailAddress);
            entity.Ignore(u => u.RoleName);
        });

        // -------------------- VEHICLE --------------------
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Id).ValueGeneratedOnAdd();
            entity.Property(v => v.Brand).IsRequired();
            entity.Property(v => v.Model).IsRequired();
            entity.Property(v => v.Transmission).IsRequired();
            entity.Property(v => v.FuelType).IsRequired();
            entity.Property(v => v.LicensePlate).IsRequired();
            entity.Property(v => v.Status).IsRequired();
            entity.Property(v => v.Description);
            entity.Property(v => v.CreatedAt);
            entity.Property(v => v.UpdatedAt);

            // Owned types para Money
            entity.OwnsOne(v => v.DailyPrice, dp =>
            {
                dp.Property(p => p.Amount).HasColumnName("DailyPrice").HasColumnType("decimal(18,2)");
                dp.Property(p => p.Currency).HasColumnName("DailyPriceCurrency");
            });
            entity.OwnsOne(v => v.DepositAmount, da =>
            {
                da.Property(p => p.Amount).HasColumnName("DepositAmount").HasColumnType("decimal(18,2)");
                da.Property(p => p.Currency).HasColumnName("DepositAmountCurrency");
            });

            // Owned type para Location
            entity.OwnsOne(v => v.Location, loc =>
            {
                loc.Property(l => l.District).HasColumnName("LocationDistrict");
                loc.Property(l => l.Address).HasColumnName("LocationAddress");
                loc.Property(l => l.Lat).HasColumnName("LocationLat");
                loc.Property(l => l.Lng).HasColumnName("LocationLng");
            });

            // Listas como JSON
            entity.Property(v => v.FeaturesJson).HasColumnType("json");
            entity.Property(v => v.RestrictionsJson).HasColumnType("json");
            entity.Property(v => v.ImagesJson).HasColumnType("json");
            
            // Ignorar propiedades de navegación
            entity.Ignore(v => v.Features);
            entity.Ignore(v => v.Restrictions);
            entity.Ignore(v => v.Images);
        });

        // -------------------- RENTAL --------------------
        modelBuilder.Entity<Rental.Domain.Model.Aggregates.Rental>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedOnAdd();
            entity.Property(r => r.VehicleId).IsRequired();
            entity.Property(r => r.RenterId).IsRequired();
            entity.Property(r => r.OwnerId).IsRequired();
            entity.Property(r => r.StartDate).IsRequired();
            entity.Property(r => r.EndDate).IsRequired();
            entity.Property(r => r.TotalPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(r => r.Status).IsRequired();
            entity.Property(r => r.PickupLocation);
            entity.Property(r => r.ReturnLocation);
            entity.Property(r => r.Notes);
            entity.Property(r => r.AdventureRouteId);
            entity.Property(r => r.VehicleRated);
            entity.Property(r => r.VehicleRating);
            entity.Property(r => r.CreatedAt);
            entity.Property(r => r.AcceptedAt);
            entity.Property(r => r.CompletedAt);
        });

        // -------------------- RENTAL INSPECTION (US12) --------------------
        modelBuilder.Entity<RentalInspection>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Id).ValueGeneratedOnAdd();
            entity.Property(i => i.RentalId).IsRequired();
            entity.Property(i => i.Type).IsRequired();
            entity.Property(i => i.PhotosJson).HasColumnType("json");
            entity.Property(i => i.Notes);
            entity.Property(i => i.CreatedById);
            entity.Property(i => i.CreatedAt);
            entity.Ignore(i => i.Photos);
            entity.HasIndex(i => i.RentalId);
        });

        // -------------------- ADVENTURE ROUTE --------------------
        modelBuilder.Entity<AdventureRoute>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired();
            entity.Property(a => a.Title).IsRequired();
            entity.Property(a => a.Type).IsRequired();
            entity.Property(a => a.Difficulty).IsRequired();
            entity.Property(a => a.EstimatedCost).HasColumnType("decimal(18,2)");
            entity.Property(a => a.PricePerSeat).HasColumnType("decimal(18,2)");
            entity.Property(a => a.Status);
            entity.Property(a => a.Tags).HasColumnType("json");
        });

        // -------------------- ROUTE PASSENGER (Carpooling US16) --------------------
        modelBuilder.Entity<RoutePassenger>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).ValueGeneratedOnAdd();
            entity.Property(p => p.RouteId).IsRequired();
            entity.Property(p => p.PassengerId).IsRequired();
            entity.Property(p => p.Status).IsRequired();
            entity.Property(p => p.Seats).IsRequired();
            entity.Property(p => p.RequestedAt);
            entity.Property(p => p.UpdatedAt);
            entity.HasIndex(p => new { p.RouteId, p.PassengerId });
        });

        // -------------------- PAYMENT --------------------
        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(p => p.Currency).IsRequired();
            entity.Property(p => p.Method).IsRequired();
            entity.Property(p => p.Status).IsRequired();
            entity.Property(p => p.Type).IsRequired();
        });

        // -------------------- WITHDRAWAL --------------------
        modelBuilder.Entity<Withdrawal>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Id).ValueGeneratedOnAdd();
            entity.Property(w => w.UserId).IsRequired();
            entity.Property(w => w.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(w => w.Method).IsRequired();
            entity.Property(w => w.Destination).IsRequired();
            entity.Property(w => w.Status).IsRequired();
            entity.Property(w => w.RejectionReason);
            entity.Property(w => w.CreatedAt);
            entity.Property(w => w.ProcessedAt);
            entity.HasIndex(w => w.UserId);
        });

        // -------------------- NOTIFICATION --------------------
        modelBuilder.Entity<NotificationEntity>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Title).IsRequired();
            entity.Property(n => n.Body).IsRequired();
            entity.Property(n => n.Type).IsRequired();
        });

        // -------------------- DEVICE TOKEN (Push/FCM) --------------------
        modelBuilder.Entity<DeviceToken>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).ValueGeneratedOnAdd();
            entity.Property(d => d.UserId).IsRequired();
            entity.Property(d => d.Token).IsRequired();
            entity.Property(d => d.Platform).IsRequired();
            entity.Property(d => d.Active);
            entity.Property(d => d.CreatedAt);
            entity.Property(d => d.UpdatedAt);
            entity.HasIndex(d => d.Token).IsUnique();
            entity.HasIndex(d => d.UserId);
        });

        // -------------------- SUPPORT TICKET --------------------
        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Subject).IsRequired();
            entity.Property(t => t.Description).IsRequired();
            entity.Property(t => t.Category).IsRequired();
            entity.Property(t => t.Status).IsRequired();
            entity.Property(t => t.Priority).IsRequired();

            entity.HasMany(t => t.Messages)
                  .WithOne(m => m.Ticket)
                  .HasForeignKey(m => m.TicketId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // -------------------- TICKET MESSAGE --------------------
        modelBuilder.Entity<TicketMessage>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Message).IsRequired();
        });

        // -------------------- REVIEW --------------------
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Rating).IsRequired();
            entity.Property(r => r.Type).IsRequired();
            entity.Property(r => r.Comment).IsRequired();
            
            entity.HasOne(r => r.Rental)
                  .WithMany()
                  .HasForeignKey(r => r.RentalId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // -------------------- USER REVIEW --------------------
        modelBuilder.Entity<UserReviewEntity>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.ReviewerId).IsRequired();
            entity.Property(r => r.ReviewedUserId).IsRequired();
            entity.Property(r => r.RentalId).IsRequired();
            entity.Property(r => r.Rating).IsRequired();
            entity.Property(r => r.Comment).IsRequired();
            entity.Property(r => r.Type).IsRequired();
            entity.Property(r => r.CreatedAt).IsRequired();
        });

        // -------------------- MESSAGE (Chat) --------------------
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Id).ValueGeneratedOnAdd();
            entity.Property(m => m.SenderId).IsRequired();
            entity.Property(m => m.ReceiverId).IsRequired();
            entity.Property(m => m.Content).IsRequired();
            entity.Property(m => m.Read);
            entity.Property(m => m.CreatedAt);
            entity.Property(m => m.ReadAt);
            entity.HasIndex(m => new { m.SenderId, m.ReceiverId });
        });
    }
}