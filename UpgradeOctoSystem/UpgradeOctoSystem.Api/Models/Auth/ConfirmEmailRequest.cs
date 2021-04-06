using System.ComponentModel.DataAnnotations;

namespace UpgradeOctoSystem.Api.Models.Auth
{
    public class ConfirmEmailRequest
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Token { get; set; }
    }
}