using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using UpgradeOctoSystem.Abstractions.Models.Auth;
using UpgradeOctoSystem.Api.Constants;

namespace UpgradeOctoSystem.Api.Models.Auth
{
    [JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
    public class LoginRequest : ILoginRequest
    {
        [Required(ErrorMessage = ValidationRules.Required)]
        [StringLength(254, MinimumLength = 3, ErrorMessage = ValidationRules.MinLength)]
        [EmailAddress(ErrorMessage = ValidationRules.InvalidEmail)]
        public string Email { get; set; }

        [Required(ErrorMessage = ValidationRules.Required)]
        public string Password { get; set; }
    }
}