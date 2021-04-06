using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using UpgradeOctoSystem.Abstractions.Models.Auth;
using UpgradeOctoSystem.Api.Constants;

namespace UpgradeOctoSystem.Api.Models.Auth
{
    public class RegisterRequest : IRegisterRequest
    {
        [Required(ErrorMessage = ValidationRules.Required)]
        [StringLength(254, MinimumLength = 3, ErrorMessage = ValidationRules.MinLength)]
        [EmailAddress(ErrorMessage = ValidationRules.InvalidEmail)]
        public string Email { get; set; }

        [Required(ErrorMessage = ValidationRules.Required)]
        [StringLength(512, MinimumLength = 8, ErrorMessage = ValidationRules.MinLength)]
        public string Password { get; set; }

        [Required(ErrorMessage = ValidationRules.Required)]
        [StringLength(512, MinimumLength = 8, ErrorMessage = ValidationRules.MinLength)]
        public string ConfirmPassword { get; set; }

        [Phone(ErrorMessage = ValidationRules.InvalidPhone)]
        public string PhoneNumber { get; set; }
    }
}