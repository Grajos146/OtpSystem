namespace OtpSystem.Domain.Interfaces;

public interface IEmailServices
{
    Task SendOtpEmailAsync(string email, string code);
}
