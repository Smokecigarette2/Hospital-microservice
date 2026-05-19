namespace Hospital.Contracts.Events;

public class AppointmentCreatedEvent
{
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
}