namespace Hospital.Booking.Api.Dtos;

public class CreateAppointmentDto
{
    public int DoctorId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }
}