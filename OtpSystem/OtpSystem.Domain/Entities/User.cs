namespace OtpSystem.Domain.Entities;

public class User
{
    // Properties
    public Guid Id { get; set; } // Primary key
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public ICollection<Otp> Otps { get; set; } = new List<Otp>();
}
