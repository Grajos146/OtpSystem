using System;
using OtpSystem.Domain.Entities;

namespace OtpSystem.Domain.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
