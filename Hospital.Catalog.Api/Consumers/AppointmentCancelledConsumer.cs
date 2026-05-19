using Hospital.Catalog.Api.Services;
using Hospital.Contracts.Events;
using MassTransit;

namespace Hospital.Catalog.Api.Consumers;

public class AppointmentCancelledConsumer : IConsumer<AppointmentCancelledEvent>
{
    private readonly DoctorService _doctorService;

    public AppointmentCancelledConsumer(DoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    public async Task Consume(ConsumeContext<AppointmentCancelledEvent> context)
    {
        await _doctorService.SetAvailabilityAsync(context.Message.DoctorId, isAvailable: true);
    }
}
