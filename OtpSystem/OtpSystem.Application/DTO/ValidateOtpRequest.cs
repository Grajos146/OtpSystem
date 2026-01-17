namespace OtpSystem.Application.DTO;

public class ValidateOtpRequest
{
    public string Email { get; set; } = null!;

    public string CorrelationId { get; set; } = string.Empty;

    public string Code { get; set; } = null!;
}
