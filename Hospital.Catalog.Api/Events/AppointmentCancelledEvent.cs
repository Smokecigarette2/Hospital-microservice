namespace Hospital.Contracts.Events;

public class AppointmentCancelledEvent
{
    public int AppointmentId { get; set; }
    public int DoctorId { get; set; }
}
