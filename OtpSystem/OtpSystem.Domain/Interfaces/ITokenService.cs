namespace OtpSystem.Domain.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
