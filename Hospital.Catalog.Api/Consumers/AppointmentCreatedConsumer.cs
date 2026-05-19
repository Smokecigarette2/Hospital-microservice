using Hospital.Catalog.Api.Data;
using Hospital.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Catalog.Api.Consumers;

public class AppointmentCreatedConsumer : IConsumer<AppointmentCreatedEvent>
{
    private readonly CatalogDbContext _context;

    public AppointmentCreatedConsumer(CatalogDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<AppointmentCreatedEvent> context)
    {
        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.Id == context.Message.DoctorId);

        if (doctor == null)
        {
            return;
        }

        doctor.IsAvailable = false;

        await _context.SaveChangesAsync();
    }
}