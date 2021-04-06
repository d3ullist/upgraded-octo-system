using System;

namespace UpgradeOctoSystem.Abstractions.Models
{
    public interface IAuthResponse
    {
        string AccessToken { get; set; }
        DateTime? ExpireDate { get; set; }
        string RefreshToken { get; set; }
    }
}