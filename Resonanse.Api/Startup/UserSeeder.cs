using Microsoft.EntityFrameworkCore;
using Resonanse.Domain.Entities;
using Resonanse.Domain.Enums;
using Resonanse.Infrastructure.Persistence;
using Resonanse.Application.Abstractions;

namespace Resonanse.Api.Startup;

public static class UserSeeder
{
    /// <summary>
    /// Создаёт админа из конфига, если ни одного пользователя ещё нет.
    /// Это bootstrap для первичной настройки.
    /// </summary>
    public static async Task EnsureAdminAsync(IServiceProvider services, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException(
                "Resonanse:Admin:Username and Password must be set for initial bootstrap.");

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ResonanseDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var anyUser = await db.Users.AnyAsync();
        if (anyUser) return;

        var admin = new User
        {
            Username = username.Trim(),
            PasswordHash = hasher.Hash(password),
            Role = UserRole.Admin
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}