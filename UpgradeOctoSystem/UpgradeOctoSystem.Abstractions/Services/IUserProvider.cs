using System.Threading.Tasks;
using UpgradeOctoSystem.Abstractions.Models;
using UpgradeOctoSystem.Abstractions.Models.Auth;

namespace UpgradeOctoSystem.Abstractions.Services
{
    public interface IUserProvider
    {
        Task ConfirmEmailAsync(string userId, string token);

        Task<IAuthResponse> LoginUserAsync(ILoginRequest model);

        Task RegisterUserAsync(IRegisterRequest model);
    }
}