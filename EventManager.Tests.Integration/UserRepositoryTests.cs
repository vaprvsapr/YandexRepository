using EventManager.Domain.Models;
using EventManager.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventManager.Tests.Integration;

[Collection("Postgres collection")]
public class UserRepositoryTests(PostgresFixture postgresFixture) : PostgresTest(postgresFixture)
{
    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_ShouldCreateUser()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new UserRepository(actContext);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "unique_login",
            PasswordHash = "hash",
            Role = UserRole.User
        };

        // Act
        var created = await repository.CreateAsync(user);

        // Assert
        Assert.NotNull(created);
        Assert.Equal(user.Id, created.Id);
        Assert.Equal("unique_login", created.Login);
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_WithDuplicateLogin_ShouldThrowDbUpdateException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();

        var repository = new UserRepository(actContext);

        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Login = "duplicate_login",
            PasswordHash = "hash1",
            Role = UserRole.User
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Login = "duplicate_login", // тот же логин
            PasswordHash = "hash2",
            Role = UserRole.Admin
        };

        arrangeContext.Users.Add(user1);
        await arrangeContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await repository.CreateAsync(user2);
        });
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task GetByLoginAsync_WithValidLogin_ReturnsUser()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new UserRepository(actContext);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "test_login",
            PasswordHash = "hash",
            Role = UserRole.User
        };

        arrangeContext.Users.Add(user);
        await arrangeContext.SaveChangesAsync();

        // Act
        var result = await repository.GetByLoginAsync("test_login");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task GetByLoginAsync_WithInvalidLogin_ThrowsKeyNotFoundException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new UserRepository(actContext);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await repository.GetByLoginAsync("unknown_login");
        });
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new UserRepository(actContext);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "old_login",
            PasswordHash = "old_hash",
            Role = UserRole.User
        };

        arrangeContext.Users.Add(user);
        await arrangeContext.SaveChangesAsync();

        // Act
        user.Login = "new_login";
        user.PasswordHash = "new_hash";
        user.Role = UserRole.Admin;

        var updated = await repository.UpdateAsync(user);

        // Assert
        Assert.Equal("new_login", updated.Login);
        Assert.Equal("new_hash", updated.PasswordHash);
        Assert.Equal(UserRole.Admin, updated.Role);
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task UpdateAsync_WithDuplicateLogin_ShouldThrowDbUpdateException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new UserRepository(actContext);

        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Login = "login1",
            PasswordHash = "hash1",
            Role = UserRole.User
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Login = "login2",
            PasswordHash = "hash2",
            Role = UserRole.User
        };

        arrangeContext.Users.AddRange(user1, user2);
        await arrangeContext.SaveChangesAsync();

        // Act
        user2.Login = "login1"; // пытаемся установить логин, который уже занят

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () =>
        {
            await repository.UpdateAsync(user2);
        });
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task DeleteAsync_ShouldDeleteUser()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var arrangeContext = await CreateContextAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new UserRepository(actContext);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = "delete_me",
            PasswordHash = "hash",
            Role = UserRole.User
        };

        arrangeContext.Users.Add(user);
        await arrangeContext.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(user.Id);

        // Assert
        var deleted = await actContext.Users.FindAsync(user.Id);
        Assert.Null(deleted);
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task DeleteAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        await ResetDatabaseAsync();
        await using var actContext = await CreateContextAsync();
        var repository = new UserRepository(actContext);

        var id = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
        {
            await repository.DeleteAsync(id);
        });
    }
}
