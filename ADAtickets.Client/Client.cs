/*
 * ADAtickets is a simple, lightweight, open source ticketing system
 * interacting with your enterprise's repositories on Azure DevOps 
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
using ADAtickets.Shared.Dtos.Requests;
using ADAtickets.Shared.Dtos.Responses;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Abstractions;
using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;

namespace ADAtickets.Client
{
    /// <summary>
    /// Base class for ADAtickets-interacting client classes.
    /// </summary>
    /// <typeparam name="TResponse">The expected returned entity from an API call.</typeparam>
    /// <typeparam name="TRequest">The type of entity to be sent to the API in the request body.</typeparam>
    /// <param name="authenticationStateProvider">Object providing the authentication state of the user to call APIs on his behalf.</param>
    /// <param name="downstreamApi">Object providing the necessary data to make API calls.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class Client<TResponse, TRequest>(AuthenticationStateProvider authenticationStateProvider, IDownstreamApi downstreamApi)
        where TResponse : ResponseDto
        where TRequest : RequestDto
    {
        private readonly IDownstreamApi _downstreamApi = downstreamApi;

        private readonly AuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;

        private string Endpoint => $"v{Service.APIVersion}/{ControllerName}";

        /// <summary>
        /// The endpoint of the controller to interact with.
        /// </summary>
        protected abstract string ControllerName { get; }

        /// <summary>
        /// Fetch a specific entity.
        /// </summary>
        /// <param name="id">Identifier of the entity to fetch.</param>
        /// <returns>A tuple containing the <see cref="HttpStatusCode"/> and the requested entity if it exists.</returns>
        public async Task<(HttpStatusCode, TResponse?)> GetAsync(Guid id)
        {
            // Fetch the logged in user
            var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Call the APIs with the permissions granted to the user
            var response = await _downstreamApi.CallApiForUserAsync(
                serviceName: Service.API,
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Get);
                    options.RelativePath = $"{Endpoint}/{id}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = Scheme.PolicySchemeDefault;
                },
                user: user);

            return (response.StatusCode, response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<TResponse>()
                : null);
        }

        /// <summary>
        /// Fetch all the entities or all the entities respecting the given criteria.
        /// </summary>
        /// <param name="filters">A group of key-value pairs defining the property name and value entities should be filtered by.</param>
        /// <returns>A tuple containing the <see cref="HttpStatusCode"/> and the collection of entities.</returns>
        public async Task<(HttpStatusCode, IEnumerable<TResponse>)> GetAllAsync(IEnumerable<KeyValuePair<string, string>>? filters = null)
        {
            // Fetch the logged in user
            var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Build the filters from the query string if provided
            var query = filters != null
                ? "?" + string.Join("&", filters.Select(f => $"{Uri.EscapeDataString(f.Key)}={Uri.EscapeDataString(f.Value)}"))
                : string.Empty;

            // Call the APIs with the permissions granted to the user
            var response = await _downstreamApi.CallApiForUserAsync(
                serviceName: Service.API,
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Get);
                    options.RelativePath = $"{Endpoint}{query}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = Scheme.PolicySchemeDefault;
                },
                user: user);

            return (response.StatusCode, response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<IEnumerable<TResponse>>() ?? []
                : []);
        }

        /// <summary>
        /// Create a new entity.
        /// </summary>
        /// <param name="entity">Object containing the values the new entity should have.</param>
        /// <returns>A tuple containing the <see cref="HttpStatusCode"/> and the created entity if it was created.</returns>
        public async Task<(HttpStatusCode, TResponse?)> PostAsync(TRequest entity)
        {
            // Fetch the logged in user
            var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Call the APIs with the permissions granted to the user
            var response = await _downstreamApi.CallApiForUserAsync(
                serviceName: Service.API,
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Post);
                    options.ContentType = MediaTypeNames.Application.Json;
                    options.RelativePath = Endpoint;
                    options.AcquireTokenOptions.AuthenticationOptionsName = Scheme.PolicySchemeDefault;
                },
                user: user,
                content: JsonContent.Create(entity));

            return (response.StatusCode, response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<TResponse>()
                : null);
        }

        /// <summary>
        /// Update a specific entity.
        /// </summary>
        /// <param name="id">Identifier of the entity to update.</param>
        /// <param name="entity">Object containing the new values the fields should be updated to.</param>
        /// <returns>A tuple containing the <see cref="HttpStatusCode"/> and the updated entity if it was updated.</returns>
        public async Task<(HttpStatusCode, TResponse?)> PutAsync(Guid id, TRequest entity)
        {
            // Fetch the logged in user
            var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Call the APIs with the permissions granted to the user
            var response = await _downstreamApi.CallApiForUserAsync(
                serviceName: Service.API,
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Put);
                    options.ContentType = MediaTypeNames.Application.Json;
                    options.RelativePath = $"{Endpoint}/{id}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = Scheme.PolicySchemeDefault;
                },
                user: user,
                content: JsonContent.Create(entity));

            return (response.StatusCode, response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<TResponse>()
                : null);
        }

        /// <summary>
        /// Delete a specific entity.
        /// </summary>
        /// <param name="id">Identifier of the entity to delete.</param>
        /// <returns>A <see cref="HttpStatusCode"/> indicating the status of the deletion.</returns>
        public async Task<HttpStatusCode> DeleteAsync(Guid id)
        {
            // Fetch the logged in user
            var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Call the APIs with the permissions granted to the user
            var response = await _downstreamApi.CallApiForUserAsync(
                serviceName: Service.API,
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Delete);
                    options.RelativePath = $"{Endpoint}/{id}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = Scheme.PolicySchemeDefault;
                },
                user: user);

            return response.StatusCode;
        }
    }
}
