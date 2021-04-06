using System;
using UpgradeOctoSystem.Abstractions.Models;

namespace UpgradeOctoSystem.Api.Models
{
    public class AuthResponse : IAuthResponse
    {
        public string AccessToken { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string RefreshToken { get; set; }
    }
}