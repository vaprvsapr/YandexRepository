using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.PostgreSql;

namespace EventManager.IntegrationTests;

public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder(image: "postgres:16-alpine").Build();
    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public string ConnectionString => _postgres.GetConnectionString();
}
