using System.ComponentModel.DataAnnotations;

namespace Hospital.Booking.Api.Dtos;

public class UpdateAppointmentDto
{
    /// <summary>
    /// Updated doctor identifier.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int DoctorId { get; set; }

    /// <summary>
    /// Updated patient full name.
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string PatientName { get; set; } = string.Empty;

    /// <summary>
    /// Updated appointment date and time. Must be in the future.
    /// </summary>
    public DateTime AppointmentDate { get; set; }
}
