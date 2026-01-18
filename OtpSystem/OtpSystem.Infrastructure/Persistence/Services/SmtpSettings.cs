namespace OtpSystem.Infrastructure.Persistence.Services;

public class SmtpSettings
{
    public string Host { get; set; } = null!;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = "OTP System";
    public int ExpiryMinutes { get; set; } = 5;
}
