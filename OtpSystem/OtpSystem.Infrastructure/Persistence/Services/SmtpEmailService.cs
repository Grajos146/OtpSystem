using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using OtpSystem.Domain.Interfaces;

namespace OtpSystem.Infrastructure.Persistence.Services;

public class SmtpEmailService : IEmailServices
{
    private readonly SmtpSettings _settings;

    public SmtpEmailService(SmtpSettings settings)
    {
        _settings = settings;
    }

    public async Task SendOtpEmailAsync(string email, string code)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port);
        {
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
            client.EnableSsl = _settings.EnableSsl;
        }

        var message = new MailMessage()
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = "Your OTP Code",
            Body = $"Your OTP code is: {code}\nIt will expire in {_settings.ExpiryMinutes} minutes.",
            IsBodyHtml = false
        };

        message.To.Add(new MailAddress(email));

        await client.SendMailAsync(message);
    }
}
