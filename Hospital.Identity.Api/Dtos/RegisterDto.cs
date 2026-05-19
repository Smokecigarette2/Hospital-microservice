using System.ComponentModel.DataAnnotations;

namespace Hospital.Identity.Api.Dtos;

public class RegisterDto
{
    /// <summary>
    /// Unique username for the account.
    /// </summary>
    [Required]
    [MinLength(3)]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Account password. Demo minimum is 6 characters.
    /// </summary>
    [Required]
    [MinLength(6)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}
