namespace OtpSystem.Application.DTO;

public class GenerateOtpRequest
{
    public string Email { get; set; } = null!;

    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}
