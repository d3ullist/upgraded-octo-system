namespace UpgradeOctoSystem.Abstractions.Models.Auth
{
    public interface ILoginRequest
    {
        string Email { get; set; }
        string Password { get; set; }
    }
}