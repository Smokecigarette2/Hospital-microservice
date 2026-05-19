using System.ComponentModel.DataAnnotations;

namespace Hospital.Identity.Api.Dtos;

public class LoginDto
{
    /// <summary>
    /// Username registered in the Identity service.
    /// </summary>
    [Required]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Account password.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}
