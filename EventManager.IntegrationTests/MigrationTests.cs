using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EventManager.IntegrationTests;

[Collection("Postgres collection")]
public class MigrationTests(PostgresFixture postgresFixture) : PostgresTest(postgresFixture)
{
    [Fact]
    [Trait("Category", "Migrations")]
    public async Task BookingsTable_ShouldExist()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        // Берём подключение из EF Core
        await using var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        // Готовим SQL
        await using var command = connection.CreateCommand();
        command.CommandText = """
        SELECT COUNT(*)
        FROM information_schema.tables
        WHERE table_name = 'bookings';
        """;

        // Act
        var count = (long?)await command.ExecuteScalarAsync();

        // Assert
        Assert.True(count > 0);
    }

    [Fact]
    [Trait("Category", "Migrations")]
    public async Task EventsTable_ShouldExist()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        // Берём подключение из EF Core
        await using var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        // Готовим SQL
        await using var command = connection.CreateCommand();
        command.CommandText = """
        SELECT COUNT(*)
        FROM information_schema.tables
        WHERE table_name = 'events';
        """;

        // Act
        var count = (long?)await command.ExecuteScalarAsync();

        // Assert
        Assert.True(count > 0);
    }

    [Fact]
    [Trait("Category", "Migrations")]
    public async Task BookingsTable_ShouldHaveExpectedColumns()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        // Берём подключение из EF Core
        await using var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();
        // Готовим SQL
        await using var command = connection.CreateCommand();
        command.CommandText = """
        SELECT column_name, data_type, is_nullable
        FROM information_schema.columns
        WHERE table_name = 'bookings';
        """;
        // Act
        var columns = new List<(string Name, string Type, string Nullable)>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        }
        // Assert
        Assert.Contains(("id", "uuid", "NO"), columns);
        Assert.Contains(("event_id", "uuid", "NO"), columns);
        Assert.Contains(("status", "text", "NO"), columns);
        Assert.Contains(("created_at", "timestamp with time zone", "NO"), columns);
        Assert.Contains(("processed_at", "timestamp with time zone", "YES"), columns);
    }

    [Fact]
    [Trait("Category", "Migrations")]
    public async Task EventsTable_ShouldHaveExpectedColumns()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        // Берём подключение из EF Core
        await using var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();
        // Готовим SQL
        await using var command = connection.CreateCommand();
        command.CommandText = """
        SELECT column_name, data_type, is_nullable
        FROM information_schema.columns
        WHERE table_name = 'events';
        """;
        // Act
        var columns = new List<(string Name, string Type, string Nullable)>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        }
        // Assert
        Assert.Contains(("id", "uuid", "NO"), columns);
        Assert.Contains(("title", "character varying", "NO"), columns);
        Assert.Contains(("description", "character varying", "NO"), columns); // Почему не nullable?
        Assert.Contains(("start_at", "timestamp with time zone", "NO"), columns);
        Assert.Contains(("end_at", "timestamp with time zone", "NO"), columns);
        Assert.Contains(("total_seats", "integer", "NO"), columns);
        Assert.Contains(("available_seats", "integer", "NO"), columns);
    }

    [Fact]
    [Trait("Category", "Migrations")]
    public async Task BookingsTable_ShouldHaveForeignKeyToEvents()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var context = await CreateContextAsync();
        // Берём подключение из EF Core
        await using var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();
        // Готовим SQL
        await using var command = connection.CreateCommand();
        command.CommandText = """
        SELECT
            tc.constraint_name, kcu.column_name, ccu.table_name AS foreign_table_name, ccu.column_name AS foreign_column_name
        FROM
            information_schema.table_constraints AS tc
            JOIN information_schema.key_column_usage AS kcu ON tc.constraint_name = kcu.constraint_name
            JOIN information_schema.constraint_column_usage AS ccu ON ccu.constraint_name = tc.constraint_name
        WHERE
            tc.table_name = 'bookings' AND tc.constraint_type = 'FOREIGN KEY';
        """;
        // Act
        var foreignKeys = new List<(string ConstraintName, string ColumnName, string ForeignTableName, string ForeignColumnName)>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            foreignKeys.Add((
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3)
            ));
        }
        // Assert
        Assert.Contains(foreignKeys, fk =>
            fk.ColumnName == "event_id" &&
            fk.ForeignTableName == "events" &&
            fk.ForeignColumnName == "id");
    }
}