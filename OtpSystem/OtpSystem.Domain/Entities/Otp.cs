namespace OtpSystem.Domain.Entities;

public class Otp
{
    // Properties
    public Guid Id { get; set; } // Primary key
    public Guid UserId { get; set; } // Foreign key to User entity
    public string CodeHash { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public int WrongAttempts { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }
    public DateTime? InvalidatedAt { get; set; }

    // Navigation property
    public User? User { get; set; }

    // Helper methods
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;

    public bool IsLocked() => WrongAttempts >= 5;

    public bool IsInvalidated() => InvalidatedAt.HasValue;

    public bool IsUsed() => UsedAt.HasValue;

    public bool IsActive() => !IsExpired() && !IsLocked() && !IsInvalidated() && !IsUsed();

    public void RegisterWrongAttempt()
    {
        WrongAttempts++;
        if (WrongAttempts >= 5)
        {
            InvalidatedAt = DateTime.UtcNow;
        }
    }

    public void MarkAsUsed()
    {
        UsedAt = DateTime.UtcNow;

        // If for some reason we try to use it after it expired,
        // we should also mark it as invalidated.
        if (UsedAt > ExpiresAt)
        {
            InvalidatedAt = UsedAt;
        }
    }

    // Private constructor for Dapper/Persistence....changed to public 
    public Otp() { }

    public static Otp Create(
        Guid userId,
        string correlationId,
        string plainCode,
        int expiryMinutes = 5
    )
    {
        return new Otp()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CorrelationId = correlationId,
            CodeHash = HashingService.HashCode(plainCode),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            WrongAttempts = 0,
            UsedAt = null,
            InvalidatedAt = null,
        };
    }

    public bool Verify(string plainCode)
    {
        // Compare the incoming plain code's hash against the stored hash
        return CodeHash == HashingService.HashCode(plainCode);
    }
}
