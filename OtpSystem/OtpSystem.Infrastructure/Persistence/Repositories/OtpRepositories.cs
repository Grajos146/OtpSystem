namespace OtpSystem.Infrastructure.Persistence.Repositories;

public class OtpRepositories : IOtpRepository
{
    private readonly DbConnectionFactory _factory;

    public OtpRepositories(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task AddAsync(Otp otp)
    {
        const string sql = """
            INSERT INTO Otps (Id, UserId, CodeHash, CorrelationId, WrongAttempts, CreatedAt, ExpiresAt, UsedAt, InvalidatedAt)
            VALUES (@Id, @UserId, @CodeHash, @CorrelationId, @WrongAttempts, @CreatedAt, @ExpiresAt, @UsedAt, @InvalidatedAt)
            """;

        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(sql, otp);
    }

    public async Task<(IEnumerable<Otp> Items, int TotalCount)> GetActiveOtpsAsync(
        string? email,
        int pageNumber,
        int pageSize,
        DateTime currentTime
    )
    {
        var offset = (pageNumber - 1) * pageSize;

        const string sql = """
            SELECT o.*, u.* FROM Otps o
            INNER JOIN Users u ON o.UserId = u.Id
            WHERE (@Email IS NULL OR u.Email = @Email)
                AND o.UsedAt IS NULL
                AND o.InvalidatedAt IS NULL
                AND o.ExpiresAt > @CurrentTime
            ORDER BY o.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(1) FROM Otps o
            INNER JOIN Users u ON o.UserId = u.Id
            WHERE (@Email IS NULL OR u.Email = @Email)
                AND o.UsedAt IS NULL
                AND o.InvalidatedAt IS NULL
                AND o.ExpiresAt > @CurrentTime;
            """;

        using var conn = _factory.CreateConnection();
        using var multi = await conn.QueryMultipleAsync(
            sql,
            new
            {
                Email = email,
                Offset = offset,
                PageSize = pageSize,
                CurrentTime = currentTime,
            }
        );

        var rawItems = await multi.ReadAsync<dynamic>();
        var items = rawItems.Select(row =>
        {
            var otp = new Otp()
            {
                Id = row.Id,
                UserId = row.UserId,
                CorrelationId = row.CorrelationId,
                CreatedAt = row.CreatedAt,
                ExpiresAt = row.ExpiresAt,
                UsedAt = row.UsedAt,
                InvalidatedAt = row.InvalidatedAt,
            };

            otp.User = new User { Email = row.Email };
            return otp;
        });

        // var items = await multi.ReadAsync<Otp, User, Otp>(
        //     (otp, user) =>
        //     {
        //         Id = row.UserId,
        //         Email = row.Email
        //         otp.User = user;
        //         return otp;
        //     }
        // });
        // );
        var totalCount = await multi.ReadFirstAsync<int>();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<Otp> Items, int TotalCount)> GetExpiredOtpsAsync(
        string? email,
        int pageNumber,
        int pageSize
    )
    {
        var offset = (pageNumber - 1) * pageSize;

        const string sql = """
            SELECT o.*, u.* FROM Otps o
            INNER JOIN Users u ON o.UserId = u.Id
            WHERE (@Email IS NULL OR u.Email = @Email)
                AND (o.ExpiresAt <= GETUTCDATE() OR o.InvalidatedAt IS NOT NULL)
            ORDER BY o.CreatedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(1) FROM Otps o
            INNER JOIN Users u ON o.UserId = u.Id
            WHERE (@Email IS NULL OR u.Email = @Email)
                AND (o.ExpiresAt <= GETUTCDATE() OR o.InvalidatedAt IS NOT NULL);
            """;

        using var conn = _factory.CreateConnection();
        using var multi = await conn.QueryMultipleAsync(
            sql,
            new
            {
                Email = email,
                Offset = offset,
                PageSize = pageSize,
            }
        );

        var rawItems = await multi.ReadAsync<dynamic>();
        var items = rawItems.Select(row =>
        {
            var otp = new Otp()
            {
                Id = row.Id,
                UserId = row.UserId,
                CorrelationId = row.CorrelationId,
                CreatedAt = row.CreatedAt,
                ExpiresAt = row.ExpiresAt,
                UsedAt = row.UsedAt,
                InvalidatedAt = row.InvalidatedAt,
            };

            otp.User = new User { Email = row.Email };
            return otp;
        });

        var totalCount = await multi.ReadSingleAsync<int>();

        return (items, totalCount);
    }

    public async Task<Otp?> GetLatestActiveOtpsByEmailAsync(string email)
    {
        const string sql = """
                SELECT TOP 1 o.*
                FROM Otps o
                INNER JOIN Users u ON o.UserId = u.Id
                WHERE u.Email = @Email
                AND o.UsedAt IS NULL
                AND o.InvalidatedAt IS NULL
                AND o.ExpiresAt > GETUTCDATE()
                ORDER BY o.CreatedAt DESC
            """;

        using var conn = _factory.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<Otp>(sql, new { Email = email });
    }

    public async Task<Otp?> GetOtpByCorrelationIdAsync(string correlationId)
    {
        const string sql =
            "SELECT o.*, u.* FROM Otps o INNER JOIN Users u ON o.UserId = u.Id WHERE CorrelationId = @CorrelationId";

        using var conn = _factory.CreateConnection();

        var result = await conn.QueryAsync<Otp, User, Otp>(
            sql,
            (otp, user) =>
            {
                otp.User = user;
                return otp;
            },
            new { CorrelationId = correlationId }
        );

        return result.FirstOrDefault();
    }

    public async Task UpdateAsync(Otp otp)
    {
        const string sql = """
                UPDATE Otps
                SET CodeHash = @CodeHash,
                    WrongAttempts = @WrongAttempts,
                    ExpiresAt = @ExpiresAt,
                    UsedAt = @UsedAt,
                    InvalidatedAt = @InvalidatedAt
                WHERE Id = @Id
            """;

        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(sql, otp);
    }
}
