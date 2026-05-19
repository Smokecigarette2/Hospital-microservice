using System.ComponentModel.DataAnnotations;

namespace Hospital.Catalog.Api.Dtos;

public class CreateDoctorDto
{
    /// <summary>
    /// Doctor's full name.
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Medical specialization, for example Cardiology.
    /// </summary>
    [Required]
    [MaxLength(80)]
    public string Specialization { get; set; } = string.Empty;

    /// <summary>
    /// Hospital department where the doctor works.
    /// </summary>
    [Required]
    [MaxLength(80)]
    public string Department { get; set; } = string.Empty;
}
