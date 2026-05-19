using System.ComponentModel.DataAnnotations;

namespace Hospital.Booking.Api.Dtos;

public class CreateAppointmentDto
{
    /// <summary>
    /// Identifier of the doctor selected from the Catalog service.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int DoctorId { get; set; }

    /// <summary>
    /// Patient full name.
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string PatientName { get; set; } = string.Empty;

    /// <summary>
    /// Requested appointment date and time. Must be in the future.
    /// </summary>
    public DateTime AppointmentDate { get; set; }
}
