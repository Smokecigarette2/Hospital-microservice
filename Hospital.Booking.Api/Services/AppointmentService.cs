using Hospital.Booking.Api.Data;
using Hospital.Booking.Api.Dtos;
using Hospital.Booking.Api.Models;
using Microsoft.EntityFrameworkCore;
using Hospital.Contracts.Events;
using MassTransit;

namespace Hospital.Booking.Api.Services;

public class AppointmentService
{
    private readonly BookingDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public AppointmentService(BookingDbContext context, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<List<AppointmentResponseDto>> GetAllAsync()
    {
        return await _context.Appointments
            .Select(appointment => ToResponseDto(appointment))
            .ToListAsync();
    }

    public async Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto)
    {
        if (dto.AppointmentDate <= DateTime.Now)
        {
            throw new InvalidOperationException("Appointment date must be in the future.");
        }

        var appointment = new Appointment
        {
            DoctorId = dto.DoctorId,
            PatientName = dto.PatientName,
            AppointmentDate = dto.AppointmentDate,
            Status = "Created"
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();


        await _publishEndpoint.Publish(new AppointmentCreatedEvent
        {
            AppointmentId = appointment.Id,
            DoctorId = appointment.DoctorId,
            PatientName = appointment.PatientName,
            AppointmentDate = appointment.AppointmentDate
        });

        return ToResponseDto(appointment);
    }

    private static AppointmentResponseDto ToResponseDto(Appointment appointment)
    {
        return new AppointmentResponseDto
        {
            Id = appointment.Id,
            DoctorId = appointment.DoctorId,
            PatientName = appointment.PatientName,
            AppointmentDate = appointment.AppointmentDate,
            Status = appointment.Status
        };
    }
}