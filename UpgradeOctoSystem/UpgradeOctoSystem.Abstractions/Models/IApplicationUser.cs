namespace UpgradeOctoSystem.Abstractions
{
    public interface IProvidePII
    {
        string FirstName { get; set; }
        string LastName { get; set; }
        string PhoneNumber { get; set; }
        string Email { get; set; }
    }

    public interface IApplicationUser : IProvidePII
    {
    }
}