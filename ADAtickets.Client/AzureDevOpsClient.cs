/*
 * ADAtickets is a simple, lightweight, open source ticketing system
 * interacting with your enterprise repositories on Azure DevOps 
 * with a two-way synchronization.
 * Copyright (C) 2025  Andrea Lucchese
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using ADAtickets.Shared.Constants;
using ADAtickets.Shared.Dtos.Responses;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Abstractions;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ADAtickets.Client
{
    /// <summary>
    /// Provides methods to interact with ADAtickets AzureDevOpsController.
    /// </summary>
    /// <param name="authenticationStateProvider">The provider of the signed in user's data.</param>
    /// <param name="downstreamApi">The downstream API service to make requests.</param>
    public sealed class AzureDevOpsClient(AuthenticationStateProvider authenticationStateProvider, IDownstreamApi downstreamApi)
    {
        private const string endpoint = $"v{Service.APIVersion}/{Controller.AzureDevOps}";

        private readonly AuthenticationStateProvider authenticationStateProvider = authenticationStateProvider;

        private readonly IDownstreamApi downstreamApi = downstreamApi;

        private readonly JsonSerializerOptions JsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Determines if a specific user with the specified email has access to the Azure DevOps organization.
        /// </summary>
        /// <param name="email">Email of the user to check.</param>
        /// <returns>A boolean value indicating whether the user has access.</returns>
        /// <exception cref="InvalidOperationException">When the method is used when no user is logged in.</exception>
        /// <exception cref="HttpRequestException">When the API call fails.</exception>
        /// <exception cref="JsonException">When the JSON response cannot be parsed.</exception>
        public async Task<bool> GetUserDevOpsAccessAsync(string email)
        {
            // Fetch the logged in user
            ClaimsPrincipal user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Validate user authentication
            if (user.Identity is null || !user.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException("The user is not authenticated.");
            }

            // Call the APIs with the permissions granted to the user
            HttpResponseMessage response = await downstreamApi.CallApiForUserAsync(
                serviceName: Service.API,
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Get);
                    options.RelativePath = $"{endpoint}/{email}/has-access";
                    options.AcquireTokenOptions.AuthenticationOptionsName = Scheme.OpenIdConnectDefault;
                },
                user: user);

            if (response.StatusCode is HttpStatusCode.OK)
            {
                var responseEntity = await response.Content.ReadFromJsonAsync<ValueWrapper<bool>>(JsonOptions) ?? throw new JsonException("Could not parse the JSON response object.");
                return responseEntity.Value;
            }
            else throw new HttpRequestException(response.ReasonPhrase, null, response.StatusCode);
        }
    }
}
