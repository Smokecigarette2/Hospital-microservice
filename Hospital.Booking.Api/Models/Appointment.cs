namespace Hospital.Booking.Api.Models;

public class Appointment
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public DateTime AppointmentDate { get; set; }

    public string Status { get; set; } = "Created";
}
