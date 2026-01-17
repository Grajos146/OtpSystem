namespace OtpSystem.Application.DTO;

public class OtpResponseDto
{
    public Guid Id { get; set; }
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
}
