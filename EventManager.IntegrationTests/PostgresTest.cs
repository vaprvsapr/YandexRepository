using EventManager.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.IntegrationTests;

[Collection("Postgres collection")]
public abstract class PostgresTest(PostgresFixture postgresFixture)
{
    protected readonly PostgresFixture _postgresFixture = postgresFixture;

    protected async Task<AppDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql(_postgresFixture.ConnectionString)
    .Options;
        var context = new AppDbContext(options);
        context.Database.Migrate();
        return context;
    }

    protected async Task ResetDatabaseAsync()
    {
        await using var context = await CreateContextAsync();
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE events, bookings Restart IDENTITY CASCADE");
    }
}
