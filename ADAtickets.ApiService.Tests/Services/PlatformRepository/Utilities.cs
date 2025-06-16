using ADAtickets.Shared.Models;

namespace ADAtickets.ApiService.Tests.Services.PlatformRepository
{
    internal static class Utilities
    {
        public static Platform CreatePlatform(
               string name = "",
               string repositoryUrl = "")
        {
            return new Platform
            {
                Name = name,
                RepositoryUrl = repositoryUrl,
            };
        }
    }
}
