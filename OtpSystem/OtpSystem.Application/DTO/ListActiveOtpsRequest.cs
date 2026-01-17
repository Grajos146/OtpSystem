namespace OtpSystem.Application.DTO;

public class ListActiveOtpsRequest
{
    public string? Email { get; set; } = null;

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 1;
}
