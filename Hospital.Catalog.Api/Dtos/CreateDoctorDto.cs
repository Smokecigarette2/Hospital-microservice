namespace Hospital.Catalog.Api.Dtos;

public class CreateDoctorDto
{
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}