using Hospital.Booking.Api.Data;
using Hospital.Booking.Api.Dtos;
using Hospital.Booking.Api.Models;
using Hospital.Booking.Api.Services;
using Hospital.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Hospital.Booking.Tests;

public class AppointmentServiceTests
{
    [Fact]
    public async Task CreateAsync_AddsAppointment_AndPublishesCreatedEvent()
    {
        await using var context = CreateContext();
        var publisher = CreatePublisherMock();
        var service = new AppointmentService(context, publisher.Object);
        var appointmentDate = DateTime.UtcNow.AddDays(1);

        var result = await service.CreateAsync(new CreateAppointmentDto
        {
            DoctorId = 10,
            PatientName = "John Patient",
            AppointmentDate = appointmentDate
        });

        Assert.True(result.Id > 0);
        Assert.Equal(AppointmentService.CreatedStatus, result.Status);
        publisher.Verify(item => item.Publish(
            It.Is<AppointmentCreatedEvent>(message =>
                message.AppointmentId == result.Id
                && message.DoctorId == 10
                && message.PatientName == "John Patient"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_RejectsDoubleBookingForSameDoctorAndTime()
    {
        await using var context = CreateContext();
        var publisher = CreatePublisherMock();
        var appointmentDate = DateTime.UtcNow.AddDays(2);
        context.Appointments.Add(new Appointment
        {
            DoctorId = 7,
            PatientName = "Existing Patient",
            AppointmentDate = appointmentDate,
            Status = AppointmentService.CreatedStatus
        });
        await context.SaveChangesAsync();

        var service = new AppointmentService(context, publisher.Object);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateAppointmentDto
            {
                DoctorId = 7,
                PatientName = "New Patient",
                AppointmentDate = appointmentDate
            }));

        Assert.Equal("The selected doctor already has an appointment at this time.", exception.Message);
    }

    [Fact]
    public async Task CancelAsync_MarksAppointmentCancelled_AndPublishesCancelledEvent()
    {
        await using var context = CreateContext();
        var publisher = CreatePublisherMock();
        var appointment = new Appointment
        {
            DoctorId = 5,
            PatientName = "John Patient",
            AppointmentDate = DateTime.UtcNow.AddDays(3),
            Status = AppointmentService.CreatedStatus
        };
        context.Appointments.Add(appointment);
        await context.SaveChangesAsync();

        var service = new AppointmentService(context, publisher.Object);

        var result = await service.CancelAsync(appointment.Id);

        Assert.True(result);
        Assert.Equal(AppointmentService.CancelledStatus, appointment.Status);
        publisher.Verify(item => item.Publish(
            It.Is<AppointmentCancelledEvent>(message =>
                message.AppointmentId == appointment.Id
                && message.DoctorId == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static BookingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BookingDbContext(options);
    }

    private static Mock<IPublishEndpoint> CreatePublisherMock()
    {
        var publisher = new Mock<IPublishEndpoint>();
        publisher.Setup(item => item.Publish(It.IsAny<AppointmentCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        publisher.Setup(item => item.Publish(It.IsAny<AppointmentCancelledEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return publisher;
    }
}
