namespace Hospital.Booking.Api.Dtos;

public class AppointmentResponseDto
{
    /// <summary>
    /// Appointment identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Doctor identifier from the Catalog service.
    /// </summary>
    public int DoctorId { get; set; }

    /// <summary>
    /// Patient full name.
    /// </summary>
    public string PatientName { get; set; } = string.Empty;

    /// <summary>
    /// Appointment date and time.
    /// </summary>
    public DateTime AppointmentDate { get; set; }

    /// <summary>
    /// Appointment lifecycle status.
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
