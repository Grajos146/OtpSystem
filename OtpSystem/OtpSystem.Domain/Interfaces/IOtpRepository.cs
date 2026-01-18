namespace OtpSystem.Domain.Interfaces;

public interface IOtpRepository
{
    Task<Otp?> GetLatestActiveOtpsByEmailAsync(string email);
    Task<Otp?> GetOtpByCorrelationIdAsync(string correlationId);

    // Task<OtpAttempt?> GetOtpAttemptByOtpIdAsync(Guid otpId);

    Task AddAsync(Otp otp);
    Task UpdateAsync(Otp otp);
    Task<(IEnumerable<Otp> Items, int TotalCount)> GetActiveOtpsAsync(
        string? email,
        int pageNumber,
        int pageSize,
        DateTime currentTime
    );
    Task<(IEnumerable<Otp> Items, int TotalCount)> GetExpiredOtpsAsync(
        string? email,
        int pageNumber,
        int pageSize
    );

    // Task AddAttemptAsync(OtpAttempt attempt);
}
