using RegistrationPortal.Server.Entities;

namespace RegistrationPortal.Server.Services
{
    public interface ISeederService
    {
        Task SeedDataAsync();
    }
}
