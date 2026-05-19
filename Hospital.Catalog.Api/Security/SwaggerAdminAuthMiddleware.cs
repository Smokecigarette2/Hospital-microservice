using System.Security.Cryptography;
using System.Text;

namespace Hospital.Catalog.Api.Security;

public sealed class SwaggerAdminAuthMiddleware
{
    private readonly RequestDelegate _next;

    public SwaggerAdminAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        if (!context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        var expectedUsername = configuration["SwaggerAuth:Username"]
            ?? configuration["DefaultAdmin:Username"]
            ?? "admin";
        var expectedPassword = configuration["SwaggerAuth:Password"]
            ?? configuration["DefaultAdmin:Password"]
            ?? "Admin123!";

        if (IsValidBasicAuth(context, expectedUsername, expectedPassword))
        {
            await _next(context);
            return;
        }

        context.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hospital Admin Swagger\"";
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }

    private static bool IsValidBasicAuth(HttpContext context, string expectedUsername, string expectedPassword)
    {
        var authorization = context.Request.Headers.Authorization.ToString();

        if (!authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var encodedCredentials = authorization["Basic ".Length..].Trim();

        try
        {
            var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var separatorIndex = decodedCredentials.IndexOf(':');

            if (separatorIndex <= 0)
            {
                return false;
            }

            var username = decodedCredentials[..separatorIndex];
            var password = decodedCredentials[(separatorIndex + 1)..];

            return FixedEquals(username, expectedUsername) && FixedEquals(password, expectedPassword);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool FixedEquals(string actual, string expected)
    {
        var actualBytes = Encoding.UTF8.GetBytes(actual);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return actualBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }
}
