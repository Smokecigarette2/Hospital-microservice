using Hospital.Identity.Api.Models;

namespace Hospital.Identity.Api.Infrastructure;

public interface IUserStore
{
    bool TryCreate(string username, string passwordHash, string role, out AppUser user);

    bool TryGetByUsername(string username, out AppUser user);
}
