using System;

namespace UpgradeOctoSystem.Abstractions.Models.Auth
{
    public interface IResetPasswordRequest
    {
        public Guid Id { get; set; }
        string Password { get; set; }
        string Token { get; set; }
    }
}