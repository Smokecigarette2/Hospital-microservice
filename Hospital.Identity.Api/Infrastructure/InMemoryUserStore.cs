using System.Collections.Concurrent;
using Hospital.Identity.Api.Models;

namespace Hospital.Identity.Api.Infrastructure;

public sealed class InMemoryUserStore : IUserStore
{
    private readonly ConcurrentDictionary<string, AppUser> _users = new(StringComparer.OrdinalIgnoreCase);
    private int _nextId;

    public bool TryCreate(string username, string passwordHash, string role, out AppUser user)
    {
        user = new AppUser
        {
            Id = Interlocked.Increment(ref _nextId),
            Username = username.Trim(),
            PasswordHash = passwordHash,
            Role = role,
            CreatedAtUtc = DateTime.UtcNow
        };

        return _users.TryAdd(NormalizeUsername(username), user);
    }

    public bool TryGetByUsername(string username, out AppUser user)
    {
        return _users.TryGetValue(NormalizeUsername(username), out user!);
    }

    private static string NormalizeUsername(string username)
    {
        return username.Trim();
    }
}
