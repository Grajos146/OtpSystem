namespace OtpSystem.Application.Mapping;

public class Mapper
{
    public static OtpResponseDto MapToDto(Otp otp)
    {
        return new OtpResponseDto
        {
            Id = otp.Id,
            Email = otp.User?.Email ?? "No Email Found",
            CorrelationId = otp.CorrelationId,
            CreatedAt = otp.CreatedAt,
            ExpiresAt = otp.ExpiresAt,
            IsActive = otp.IsActive(),
            IsExpired = otp.IsExpired(),
        };
    }
}
