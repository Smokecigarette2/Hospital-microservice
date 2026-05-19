using Hospital.Identity.Api.Dtos;
using Hospital.Identity.Api.Infrastructure;
using Hospital.Identity.Api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Hospital.Identity.Tests;

public class AuthServiceTests
{
    [Fact]
    public void Register_CreatesUserRole_AndReturnsJwt()
    {
        var service = CreateService();

        var result = service.Register(new RegisterDto
        {
            Username = "patient",
            Password = "Password123!"
        });

        Assert.Equal("patient", result.Username);
        Assert.Equal(AuthService.UserRole, result.Role);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    [Fact]
    public void Register_RejectsDuplicateUsernames()
    {
        var service = CreateService();
        var dto = new RegisterDto
        {
            Username = "patient",
            Password = "Password123!"
        };

        service.Register(dto);

        var exception = Assert.Throws<InvalidOperationException>(() => service.Register(dto));
        Assert.Equal("Username already exists.", exception.Message);
    }

    [Fact]
    public void EnsureDefaultAdmin_SeedsAdminAccount()
    {
        var service = CreateService();
        service.EnsureDefaultAdmin();

        var result = service.Login(new LoginDto
        {
            Username = "admin",
            Password = "Admin123!"
        });

        Assert.Equal(AuthService.AdminRole, result.Role);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

    private static AuthService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "THIS_IS_A_DEMO_SECRET_KEY_FOR_HOSPITAL_PROJECT_12345",
                ["Jwt:Issuer"] = "Hospital.Identity.Api",
                ["Jwt:Audience"] = "Hospital.Client",
                ["DefaultAdmin:Username"] = "admin",
                ["DefaultAdmin:Password"] = "Admin123!"
            })
            .Build();

        return new AuthService(configuration, new InMemoryUserStore());
    }
}
