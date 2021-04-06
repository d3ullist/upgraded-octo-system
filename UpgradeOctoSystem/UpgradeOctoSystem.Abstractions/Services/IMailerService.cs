using System;
using System.Threading.Tasks;

namespace UpgradeOctoSystem.Abstractions.Services
{
    public interface IMailerService
    {
        Task SendEmailVerificationAsync(string email, string name, string verificationToken, Guid userId);

        Task SendForgotPasswordMailAsync(string email, string name, string resetToken, Guid userId);
    }
}