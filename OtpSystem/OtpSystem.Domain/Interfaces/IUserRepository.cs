using System;
using OtpSystem.Domain.Entities;

namespace OtpSystem.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);

    Task AddAsync(User user);
}
