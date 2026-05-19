using Hospital.Booking.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Booking.Api.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments { get; set; }
}