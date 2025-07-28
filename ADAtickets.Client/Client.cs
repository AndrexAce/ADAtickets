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
using ADAtickets.Shared.Dtos.Requests;
using ADAtickets.Shared.Dtos.Responses;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using System.ComponentModel;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ADAtickets.Client
{
    /// <summary>
    /// Base class for ADAtickets-interacting client classes.
    /// </summary>
    /// <typeparam name="TResponse">The expected returned entity from an API call.</typeparam>
    /// <typeparam name="TRequest">The type of entity to be sent to the API in the request body.</typeparam>
    /// <param name="authenticationStateProvider">Object providing the authentication state of the user to call APIs on his behalf.</param>
    /// <param name="downstreamApi">Object providing the necessary data to make API calls.</param>
    /// <param name="configuration">The configuration settings.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class Client<TResponse, TRequest>(AuthenticationStateProvider authenticationStateProvider, IDownstreamApi downstreamApi, IConfiguration configuration)
        where TResponse : ResponseDto
        where TRequest : RequestDto
    {
        /// <summary>
        /// Error message used when a JSON response object cannot be parsed.
        /// </summary>
        protected const string JsonExceptionMessage = "Could not parse the JSON response object.";

        /// <summary>
        /// Provides the authentication state of the user to call APIs on his behalf.
        /// </summary>
        protected readonly AuthenticationStateProvider authenticationStateProvider = authenticationStateProvider;

        /// <summary>
        /// Represents an interface to make remote API requests.
        /// </summary>
        protected readonly IDownstreamApi downstreamApi = downstreamApi;

        /// <summary>
        /// Holds the configuration settings for the applications.
        /// </summary>
        protected readonly IConfiguration configuration = configuration;

        /// <summary>
        /// The name of the controller to interact with.
        /// </summary>
        protected abstract string ControllerName { get; }

        /// <summary>
        /// The endpoint to call, including the API version. 
        /// </summary>
        protected string Endpoint => $"v{Service.APIVersion}/{ControllerName}";

        /// <summary>
        /// The serialization options used to serialize and deserialize JSON data.
        /// </summary>
        protected readonly JsonSerializerOptions JsonOptions = new()
        {
            Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Fetch a specific entity.
        /// </summary>
        /// <param name="id">Identifier of the entity to fetch.</param>
        /// <returns>The requested entity.</returns>
        /// <exception cref="InvalidOperationException">When the method is used when no user is logged in.</exception>
        /// <exception cref="HttpRequestException">When the API call fails.</exception>
        /// <exception cref="JsonException">When the JSON response cannot be parsed.</exception>
        public async Task<TResponse> GetAsync(Guid id)
        {
            // Fetch the logged in user
            ClaimsPrincipal user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Call the APIs with the permissions granted to the user
            HttpResponseMessage response = await downstreamApi.CallApiForUserAsync(
                serviceName: InferServiceName(user),
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Get);
                    options.RelativePath = $"{Endpoint}/{id}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = InferAuthenticationScheme(user);
                },
                user: user);

            if (response.StatusCode is HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions) ?? throw new JsonException(JsonExceptionMessage);
            }
            else throw new HttpRequestException(response.ReasonPhrase, null, response.StatusCode);
        }

        /// <summary>
        /// Fetch all the entities or all the entities respecting the given criteria.
        /// </summary>
        /// <param name="filters">A group of key-value pairs defining the property name and value entities should be filtered by.</param>
        /// <returns>The collection of entities.</returns>
        /// <exception cref="InvalidOperationException">When the method is used when no user is logged in.</exception>
        /// <exception cref="HttpRequestException">When the API call fails.</exception>
        /// <exception cref="JsonException">When the JSON response cannot be parsed.</exception>
        public async Task<IEnumerable<TResponse>> GetAllAsync(IEnumerable<KeyValuePair<string, string>>? filters = null)
        {
            // Fetch the logged in user
            ClaimsPrincipal user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Call the APIs with the permissions granted to the user
            HttpResponseMessage response = await downstreamApi.CallApiForUserAsync(
                serviceName: InferServiceName(user),
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Get);
                    options.RelativePath = Endpoint;
                    options.ExtraQueryParameters = filters?.ToDictionary();
                    options.AcquireTokenOptions.AuthenticationOptionsName = InferAuthenticationScheme(user);
                },
                user: user);

            if (response.StatusCode is HttpStatusCode.OK)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<TResponse>>(JsonOptions) ?? throw new JsonException(JsonExceptionMessage);
            }
            else throw new HttpRequestException(response.ReasonPhrase, null, response.StatusCode);
        }

        /// <summary>
        /// Create a new entity.
        /// </summary>
        /// <param name="entity">Object containing the values the new entity should have.</param>
        /// <returns>The created entity.</returns>
        /// <exception cref="InvalidOperationException">When the method is used when no user is logged in.</exception>
        /// <exception cref="HttpRequestException">When the API call fails.</exception>
        /// <exception cref="JsonException">When the JSON response cannot be parsed.</exception>
        public async Task<TResponse> PostAsync(TRequest entity)
        {
            // Fetch the logged in user
            ClaimsPrincipal user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Call the APIs with the permissions granted to the user
            HttpResponseMessage response = await downstreamApi.CallApiForUserAsync(
                serviceName: InferServiceName(user),
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Post);
                    options.ContentType = MediaTypeNames.Application.Json;
                    options.RelativePath = Endpoint;
                    options.AcquireTokenOptions.AuthenticationOptionsName = InferAuthenticationScheme(user);
                },
                user: user,
                content: JsonContent.Create(entity, options: JsonOptions));

            if (response.StatusCode is HttpStatusCode.Created)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions) ?? throw new JsonException(JsonExceptionMessage);
            }
            else throw new HttpRequestException(response.ReasonPhrase, null, response.StatusCode);
        }

        /// <summary>
        /// Update a specific entity.
        /// </summary>
        /// <param name="id">Identifier of the entity to update.</param>
        /// <param name="entity">Object containing the new values the fields should be updated to.</param>
        /// <returns>The entity if it was created; otherwise, <see langword="null"/> if the entity was updated.</returns>
        /// <exception cref="InvalidOperationException">When the method is used when no user is logged in.</exception>
        /// <exception cref="HttpRequestException">When the API call fails.</exception>
        /// <exception cref="JsonException">When the JSON response cannot be parsed.</exception>
        public async Task<TResponse?> PutAsync(Guid id, TRequest entity)
        {
            // Fetch the logged in user
            ClaimsPrincipal user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Call the APIs with the permissions granted to the user
            HttpResponseMessage response = await downstreamApi.CallApiForUserAsync(
                serviceName: InferServiceName(user),
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Put);
                    options.ContentType = MediaTypeNames.Application.Json;
                    options.RelativePath = $"{Endpoint}/{id}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = InferAuthenticationScheme(user);
                },
                user: user,
                content: JsonContent.Create(entity, options: JsonOptions));

            if (response.StatusCode is HttpStatusCode.Created)
            {
                return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions) ?? throw new JsonException(JsonExceptionMessage);
            }
            else if (response.StatusCode is HttpStatusCode.NoContent)
            {
                return null;
            }
            else throw new HttpRequestException(response.ReasonPhrase, null, response.StatusCode);
        }

        /// <summary>
        /// Delete a specific entity.
        /// </summary>
        /// <param name="id">Identifier of the entity to delete.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">When the method is used when no user is logged in.</exception>
        /// <exception cref="HttpRequestException">When the API call fails.</exception>
        public async Task DeleteAsync(Guid id)
        {
            // Fetch the logged in user
            ClaimsPrincipal user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Call the APIs with the permissions granted to the user
            HttpResponseMessage response = await downstreamApi.CallApiForUserAsync(
                serviceName: InferServiceName(user),
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Delete);
                    options.RelativePath = $"{Endpoint}/{id}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = InferAuthenticationScheme(user);
                },
                user: user);

            if (response.StatusCode is not HttpStatusCode.NoContent)
            {
                throw new HttpRequestException(response.ReasonPhrase, null, response.StatusCode);
            }
        }

        private static string GetUserTenantId(ClaimsPrincipal user)
        {
            if (user.Identity is null)
            {
                throw new InvalidOperationException("The user is not authenticated.");
            }

            if (!user.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException("The user is not authenticated.");
            }

            string? tid = user.GetHomeTenantId();

            return tid is null ? throw new InvalidOperationException("The user is not authenticated.") : tid;
        }

        /// <summary>
        /// Infers the appropriate API service to use based on the user's tenant ID.
        /// </summary>
        /// <remarks>This method determines which service to use by comparing the user's tenant ID with
        /// the primary tenant ID configured in the application settings. Ensure that the configuration section for the
        /// primary tenant ID is correctly set.</remarks>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> representing the user whose tenant ID is used for inference.</param>
        /// <returns>The service name as a string. Returns <see cref="Service.API"/> if the user's tenant ID matches the primary
        /// tenant ID; otherwise, returns <see cref="Service.ExternalAPI"/>.</returns>
        protected string InferServiceName(ClaimsPrincipal user)
        {
            string tenantId = GetUserTenantId(user);
            string? primaryTenantId = configuration.GetSection($"{Scheme.OpenIdConnectDefault}:TenantId").Value;

            return tenantId == primaryTenantId ? Service.API : Service.ExternalAPI;
        }

        /// <summary>
        /// Infers the appropriate authentication scheme to use based on the user's tenant ID.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> representing the user whose tenant ID is used for inference.</param>
        /// <returns>The authentication scheme as a string. Returns <see cref="Scheme.OpenIdConnectDefault"/> if the user's tenant ID matches the primary
        /// tenant ID; otherwise, returns <see cref="Scheme.ExternalOpenIdConnectDefault"/>.</returns>
        protected string InferAuthenticationScheme(ClaimsPrincipal user)
        {
            string tenantId = GetUserTenantId(user);
            string? primaryTenantId = configuration.GetSection($"{Scheme.OpenIdConnectDefault}:TenantId").Value;

            return tenantId == primaryTenantId ? Scheme.OpenIdConnectDefault : Scheme.ExternalOpenIdConnectDefault;
        }
    }
}
