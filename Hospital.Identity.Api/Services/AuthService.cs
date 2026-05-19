using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hospital.Identity.Api.Dtos;
using Hospital.Identity.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Hospital.Identity.Api.Services;

public class AuthService
{
    private readonly IConfiguration _configuration;

    private readonly ConcurrentDictionary<string, AppUser> _users = new();
    private int _nextId = 0;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthResponseDto Register(RegisterDto dto)
    {
        var role = dto.Role == "Admin" ? "Admin" : "User";

        var user = new AppUser
        {
            Id = Interlocked.Increment(ref _nextId),
            Username = dto.Username,
            Password = dto.Password,
            Role = role
        };

        if (!_users.TryAdd(dto.Username, user))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        var token = GenerateToken(user);

        return new AuthResponseDto
        {
            Username = user.Username,
            Role = user.Role,
            Token = token
        };
    }

    public AuthResponseDto Login(LoginDto dto)
    {
        if (!_users.TryGetValue(dto.Username, out var user) || user.Password != dto.Password)
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        var token = GenerateToken(user);

        return new AuthResponseDto
        {
            Username = user.Username,
            Role = user.Role,
            Token = token
        };
    }

    private string GenerateToken(AppUser user)
    {
        var key = _configuration["Jwt:Key"]!;
        var issuer = _configuration["Jwt:Issuer"]!;
        var audience = _configuration["Jwt:Audience"]!;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}