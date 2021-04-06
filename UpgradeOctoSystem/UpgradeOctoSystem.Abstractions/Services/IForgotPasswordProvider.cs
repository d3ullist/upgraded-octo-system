using System.Threading.Tasks;
using UpgradeOctoSystem.Abstractions.Models.Auth;

namespace UpgradeOctoSystem.Abstractions.Services
{
    public interface IForgotPasswordProvider
    {
        Task ForgetPasswordAsync(string email);

        Task ResetPasswordAsync(IResetPasswordRequest model);
    }
}