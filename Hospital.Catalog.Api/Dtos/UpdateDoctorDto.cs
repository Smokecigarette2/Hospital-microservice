using System.ComponentModel.DataAnnotations;

namespace Hospital.Catalog.Api.Dtos;

public class UpdateDoctorDto
{
    /// <summary>
    /// Updated doctor full name.
    /// </summary>
    [Required]
    [MaxLength(120)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Updated medical specialization.
    /// </summary>
    [Required]
    [MaxLength(80)]
    public string Specialization { get; set; } = string.Empty;

    /// <summary>
    /// Updated department.
    /// </summary>
    [Required]
    [MaxLength(80)]
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Availability flag used by booking events and administrators.
    /// </summary>
    public bool IsAvailable { get; set; }
}
