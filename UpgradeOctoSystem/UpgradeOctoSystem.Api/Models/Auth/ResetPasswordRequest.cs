using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.ComponentModel.DataAnnotations;
using UpgradeOctoSystem.Abstractions.Models.Auth;
using UpgradeOctoSystem.Api.Constants;

namespace UpgradeOctoSystem.Api.Models.Auth
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class ResetPasswordRequest : IResetPasswordRequest
    {
        [Required(ErrorMessage = ValidationRules.Required)]
        public Guid Id { get; set; }

        [Required(ErrorMessage = ValidationRules.Required)]
        public string Token { get; set; }

        [Required(ErrorMessage = ValidationRules.Required)]
        [StringLength(512, MinimumLength = 8, ErrorMessage = ValidationRules.MinLength)]
        public string Password { get; set; }

        [Required(ErrorMessage = ValidationRules.Required)]
        [StringLength(512, MinimumLength = 8, ErrorMessage = ValidationRules.MinLength)]
        public string ConfirmPassword { get; set; }
    }
}