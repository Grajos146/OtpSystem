using Dapper;
using OtpSystem.Domain.Entities;
using OtpSystem.Domain.Interfaces;

namespace OtpSystem.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _factory;

    public UserRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task AddAsync(User user)
    {
        const string sql = """
            INSERT INTO Users (Id, Email, CreatedAt, IsActive)
            VALUES (@Id, @Email, @CreatedAt, @IsActive)
            """;

        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(sql, user);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        const string sql = """
            SELECT *
            FROM Users
            WHERE Email = @Email
            """;

        using var conn = _factory.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }
}
