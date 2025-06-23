using ADAtickets.Shared.Constants;
using ADAtickets.Shared.Dtos.Responses;
using ADAtickets.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Abstractions;
using System.Net;
using System.Net.Http.Json;

namespace ADAtickets.Client
{
    /// <summary>
    /// Provides methods to interact with ADAtickets AzurDevOpsController.
    /// </summary>
    /// <param name="authenticationStateProvider">The provider of the signed in user's data.</param>
    /// <param name="downstreamApi">The downstream API service to make requests.</param>
    public sealed class AzureDevOpsClient(AuthenticationStateProvider authenticationStateProvider, IDownstreamApi downstreamApi)
    {
        private const string endpoint = $"v{Service.APIVersion}/{Controller.AzureDevOps}";

        /// <summary>
        /// Determines if a specific <see cref="User"/> entity with <paramref name="email"/> has access to the Azure DevOps organization.
        /// </summary>
        /// <param name="email">Email of the <see cref="User"/> entity to check.</param>
        /// <returns>A tuple containing the <see cref="HttpStatusCode"/> and a <see cref="ValueWrapper{TValue}"/> indicating access.</returns>
        /// <exception cref="OperationCanceledException">When the operation is canceled.</exception>
        public async Task<(HttpStatusCode, ValueWrapper<bool>?)> GetUserDevOpsAccessAsync(string email)
        {
            // Use the downstream API to call the backend endpoint
            var response = await downstreamApi.CallApiForUserAsync(
                serviceName: Service.API,
                options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Get);
                    options.RelativePath = $"{endpoint}/{email}/has-access";
                    options.AcquireTokenOptions.AuthenticationOptionsName = Scheme.OpenIdConnectDefault;
                }, (await authenticationStateProvider.GetAuthenticationStateAsync()).User);

            // Deserialize the response as ValueWrapper<bool>
            return (response.StatusCode, response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ValueWrapper<bool>>() : null);
        }

    }
}
