using Microsoft.EntityFrameworkCore;
using Resonanse.Infrastructure.Persistence;

namespace Resonanse.UnitTests.Infrastructure;

/// <summary>
/// Создаёт изолированный in-memory DbContext для каждого теста.
/// Использует уникальное имя БД, чтобы тесты не пересекались.
/// </summary>
public static class TestDbContextFactory
{
    public static ResonanseDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ResonanseDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ResonanseDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}