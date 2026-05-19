namespace Hospital.Catalog.Api.Dtos;

public class DoctorResponseDto
{
    /// <summary>
    /// Doctor identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Doctor's full name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Medical specialization.
    /// </summary>
    public string Specialization { get; set; } = string.Empty;

    /// <summary>
    /// Hospital department.
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the doctor can accept a new appointment.
    /// </summary>
    public bool IsAvailable { get; set; }
}
