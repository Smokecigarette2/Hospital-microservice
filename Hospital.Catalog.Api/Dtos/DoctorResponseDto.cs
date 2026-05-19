namespace Hospital.Catalog.Api.Dtos;

public class DoctorResponseDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
}