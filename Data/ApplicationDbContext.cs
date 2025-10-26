using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem2.Models;

namespace HotelManagementSystem2.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure decimal precision for currency
            builder.Entity<Room>()
                .Property(r => r.PricePerNight)
                .HasPrecision(18, 2);

            builder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<Booking>()
                .Property(b => b.AmountPaid)
                .HasPrecision(18, 2);

            builder.Entity<Booking>()
                .Property(b => b.Balance)
                .HasPrecision(18, 2);

            // Configure relationships
            builder.Entity<Booking>()
                .HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
