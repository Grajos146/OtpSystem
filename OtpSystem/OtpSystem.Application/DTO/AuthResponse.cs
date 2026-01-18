namespace OtpSystem.Application.DTO;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public OtpValidationStatus Status { get; set; }
}
