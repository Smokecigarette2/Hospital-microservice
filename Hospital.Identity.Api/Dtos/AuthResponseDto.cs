namespace Hospital.Identity.Api.Dtos;

public class AuthResponseDto
{
    /// <summary>
    /// Authenticated username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Role assigned to the user: Admin or User.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// JWT bearer token used to call protected APIs.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
