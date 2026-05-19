using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Hospital.Identity.Api.Dtos;
using Hospital.Identity.Api.Infrastructure;
using Hospital.Identity.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace Hospital.Identity.Api.Services;

public class AuthService
{
    public const string AdminRole = "Admin";
    public const string UserRole = "User";

    private readonly IConfiguration _configuration;
    private readonly IUserStore _users;

    public AuthService(IConfiguration configuration, IUserStore users)
    {
        _configuration = configuration;
        _users = users;
    }

    public AuthResponseDto Register(RegisterDto dto)
    {
        return CreateAccount(dto.Username, dto.Password, UserRole);
    }

    public AuthResponseDto RegisterAdmin(RegisterDto dto)
    {
        return CreateAccount(dto.Username, dto.Password, AdminRole);
    }

    public AuthResponseDto Login(LoginDto dto)
    {
        var username = NormalizeUsername(dto.Username);

        if (!_users.TryGetByUsername(username, out var user)
            || user.PasswordHash != HashPassword(username, dto.Password))
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        return CreateResponse(user);
    }

    public void EnsureDefaultAdmin()
    {
        var username = NormalizeUsername(_configuration["DefaultAdmin:Username"] ?? "admin");
        var password = _configuration["DefaultAdmin:Password"] ?? "Admin123!";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Default admin username and password must be configured.");
        }

        if (_users.TryGetByUsername(username, out _))
        {
            return;
        }

        _users.TryCreate(username, HashPassword(username, password), AdminRole, out _);
    }

    private AuthResponseDto CreateAccount(string rawUsername, string password, string role)
    {
        var username = NormalizeUsername(rawUsername);

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Password is required.");
        }

        var passwordHash = HashPassword(username, password);

        if (!_users.TryCreate(username, passwordHash, role, out var user))
        {
            throw new InvalidOperationException("Username already exists.");
        }

        return CreateResponse(user);
    }

    private AuthResponseDto CreateResponse(AppUser user)
    {
        return new AuthResponseDto
        {
            Username = user.Username,
            Role = user.Role,
            Token = GenerateToken(user)
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
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string username, string password)
    {
        var bytes = Encoding.UTF8.GetBytes($"{NormalizeUsername(username)}:{password}");
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private static string NormalizeUsername(string username)
    {
        return username.Trim();
    }
}
