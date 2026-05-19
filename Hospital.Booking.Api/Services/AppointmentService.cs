using Hospital.Booking.Api.Data;
using Hospital.Booking.Api.Dtos;
using Hospital.Booking.Api.Models;
using Hospital.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Booking.Api.Services;

public class AppointmentService
{
    public const string CreatedStatus = "Created";
    public const string CancelledStatus = "Cancelled";

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
            .AsNoTracking()
            .OrderByDescending(appointment => appointment.AppointmentDate)
            .Select(appointment => ToResponseDto(appointment))
            .ToListAsync();
    }

    public async Task<AppointmentResponseDto?> GetByIdAsync(int id)
    {
        var appointment = await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        return appointment is null ? null : ToResponseDto(appointment);
    }

    public async Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto)
    {
        ValidateAppointment(dto.DoctorId, dto.PatientName, dto.AppointmentDate);
        await EnsureDoctorSlotIsFreeAsync(dto.DoctorId, dto.AppointmentDate);

        var appointment = new Appointment
        {
            DoctorId = dto.DoctorId,
            PatientName = dto.PatientName.Trim(),
            AppointmentDate = dto.AppointmentDate,
            Status = CreatedStatus
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        await PublishCreatedAsync(appointment);

        return ToResponseDto(appointment);
    }

    public async Task<bool> UpdateAsync(int id, UpdateAppointmentDto dto)
    {
        ValidateAppointment(dto.DoctorId, dto.PatientName, dto.AppointmentDate);

        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment is null)
        {
            return false;
        }

        if (appointment.Status == CancelledStatus)
        {
            throw new InvalidOperationException("Cancelled appointments cannot be updated.");
        }

        await EnsureDoctorSlotIsFreeAsync(dto.DoctorId, dto.AppointmentDate, id);

        var previousDoctorId = appointment.DoctorId;

        appointment.DoctorId = dto.DoctorId;
        appointment.PatientName = dto.PatientName.Trim();
        appointment.AppointmentDate = dto.AppointmentDate;

        await _context.SaveChangesAsync();

        if (previousDoctorId != appointment.DoctorId)
        {
            await PublishCancelledAsync(id, previousDoctorId);
            await PublishCreatedAsync(appointment);
        }

        return true;
    }

    public async Task<bool> CancelAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment is null)
        {
            return false;
        }

        if (appointment.Status == CancelledStatus)
        {
            return true;
        }

        appointment.Status = CancelledStatus;

        await _context.SaveChangesAsync();
        await PublishCancelledAsync(appointment.Id, appointment.DoctorId);

        return true;
    }

    private async Task EnsureDoctorSlotIsFreeAsync(int doctorId, DateTime appointmentDate, int? ignoredAppointmentId = null)
    {
        var hasConflict = await _context.Appointments.AnyAsync(appointment =>
            appointment.DoctorId == doctorId
            && appointment.AppointmentDate == appointmentDate
            && appointment.Status != CancelledStatus
            && (!ignoredAppointmentId.HasValue || appointment.Id != ignoredAppointmentId.Value));

        if (hasConflict)
        {
            throw new InvalidOperationException("The selected doctor already has an appointment at this time.");
        }
    }

    private async Task PublishCreatedAsync(Appointment appointment)
    {
        await _publishEndpoint.Publish(new AppointmentCreatedEvent
        {
            AppointmentId = appointment.Id,
            DoctorId = appointment.DoctorId,
            PatientName = appointment.PatientName,
            AppointmentDate = appointment.AppointmentDate
        });
    }

    private async Task PublishCancelledAsync(int appointmentId, int doctorId)
    {
        await _publishEndpoint.Publish(new AppointmentCancelledEvent
        {
            AppointmentId = appointmentId,
            DoctorId = doctorId
        });
    }

    private static void ValidateAppointment(int doctorId, string patientName, DateTime appointmentDate)
    {
        if (doctorId <= 0)
        {
            throw new InvalidOperationException("Doctor id must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(patientName))
        {
            throw new InvalidOperationException("Patient name is required.");
        }

        if (appointmentDate <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Appointment date must be in the future.");
        }
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
