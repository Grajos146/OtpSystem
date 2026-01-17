namespace OtpSystem.Application.DTO;

public class ListExpiredOtpsRequest
{
    public string? Email { get; set; } = null;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 1;
}
