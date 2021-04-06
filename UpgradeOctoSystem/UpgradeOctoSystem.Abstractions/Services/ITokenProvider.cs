#if NET5_0_OR_GREATER
using Microsoft.AspNetCore.Identity;
#endif

using System.Threading.Tasks;
using UpgradeOctoSystem.Abstractions.Models;

namespace UpgradeOctoSystem.Abstractions.Services
{
    public interface ITokenProvider
    {
#if NET5_0_OR_GREATER
        Task<string> GenerateAccessTokenAsync(IdentityUser user);
#endif

        Task<IAuthResponse> RefreshTokenAsync(IAuthResponse model);
    }
}