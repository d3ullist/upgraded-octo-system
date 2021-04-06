namespace UpgradeOctoSystem.Abstractions.Models.Auth
{
    public interface IRegisterRequest
    {
        string Email { get; set; }

        string Password { get; set; }

        string PhoneNumber { get; set; }
    }
}