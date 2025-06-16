using ADAtickets.Shared.Models;

namespace ADAtickets.ApiService.Tests.Services.UserRepository
{
    internal static class Utilities
    {
        public static User CreateUser(
               string name = "",
               string surname = "",
               string? microsoftAccountId = "")
        {
            return new User
            {
                Name = name,
                Surname = surname,
                MicrosoftAccountId = microsoftAccountId
            };
        }
    }
}
