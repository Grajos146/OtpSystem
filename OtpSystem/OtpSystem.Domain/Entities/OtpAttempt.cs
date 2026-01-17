// namespace OtpSystem.Domain.Entities;

// public class OtpAttempt
// {
//     // Properties
//     public Guid Id { get; set; } // Primary key
//     public Guid OtpId { get; set; } // Foreign key to Otp entity

//     public string AttemptedCode { get; set; } = string.Empty;
//     public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
//     public bool IsSuccessful { get; set; }

//     // Navigation property
//     public Otp? Otp { get; set; }

//     private OtpAttempt() { }

//     public static OtpAttempt Attempted(Guid otpId, string attemptedCode, bool isSuccessful)
//     {
//         return new OtpAttempt
//         {
//             Id = Guid.NewGuid(),
//             OtpId = otpId,
//             AttemptedCode = attemptedCode,
//             AttemptedAt = DateTime.UtcNow,
//             IsSuccessful = isSuccessful,
//         };
//     }
// }
