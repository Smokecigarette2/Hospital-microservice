using Hospital.Catalog.Api.Services;
using Hospital.Contracts.Events;
using MassTransit;

namespace Hospital.Catalog.Api.Consumers;

public class AppointmentCreatedConsumer : IConsumer<AppointmentCreatedEvent>
{
    private readonly DoctorService _doctorService;

    public AppointmentCreatedConsumer(DoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    public async Task Consume(ConsumeContext<AppointmentCreatedEvent> context)
    {
        await _doctorService.SetAvailabilityAsync(context.Message.DoctorId, isAvailable: false);
    }
}
