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
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Abstractions;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Security.Claims;

namespace ADAtickets.Client
{
    /// <summary>
    /// Provides methods to interact with ADAtickets UsersController.
    /// </summary>
    /// <param name="authenticationStateProvider">The provider of the signed in user's data.</param>
    /// <param name="downstreamApi">The downstream API service to make requests.</param>
    /// <param name="configuration">The configuration settings.</param>
    public sealed class UsersClient(AuthenticationStateProvider authenticationStateProvider, IDownstreamApi downstreamApi, IConfiguration configuration)
        : Client<UserResponseDto, UserRequestDto>(authenticationStateProvider, downstreamApi, configuration)
    {
        /// <inheritdoc cref="Client{TResponse, TRequest}.ControllerName"/>
        protected override string ControllerName => Controller.Users;

        /// <summary>
        /// Fetch a specific entity.
        /// </summary>
        /// <param name="email">Email of the entity to fetch.</param>
        /// <returns>A tuple containing the <see cref="HttpStatusCode"/> and the requested entity if it exists.</returns>
        /// <exception cref="InvalidOperationException">When the method is used when no user is logged in.</exception>
        /// <seealso cref="Client{TResponse, TRequest}.GetAsync(Guid)"/>
        public async Task<(HttpStatusCode, UserResponseDto?)> GetAsync(string email)
        {
            // Fetch the logged in user
            ClaimsPrincipal user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Call the APIs with the permissions granted to the user
            HttpResponseMessage response = await downstreamApi.CallApiForUserAsync(
                serviceName: InferServiceName(user),
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Get);
                    options.RelativePath = $"{Endpoint}/{email}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = InferAuthenticationScheme(user);
                },
                user: user);

            UserResponseDto? responseEntity = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<UserResponseDto>(JsonOptions) : null;

            InvokeResponseHandler(response);

            return (response.StatusCode, responseEntity);
        }

        /// <summary>
        /// Update a specific entity.
        /// </summary>
        /// <param name="email">Email of the entity to update.</param>
        /// <param name="entity">Object containing the new values the fields should be updated to.</param>
        /// <returns>A tuple containing the <see cref="HttpStatusCode"/> and the updated entity if it was updated.</returns>
        /// <exception cref="InvalidOperationException">When the method is used when no user is logged in.</exception>
        /// <seealso cref="Client{TResponse, TRequest}.PutAsync(Guid, TRequest)"/>
        public async Task<(HttpStatusCode, UserResponseDto?)> PutAsync(string email, UserRequestDto entity)
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
                    options.RelativePath = $"{Endpoint}/{email}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = InferAuthenticationScheme(user);
                },
                user: user,
                content: JsonContent.Create(entity, options: JsonOptions));

            UserResponseDto? responseEntity = response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<UserResponseDto>(JsonOptions) : null;

            InvokeResponseHandler(response);

            return (response.StatusCode, responseEntity);
        }

        /// <summary>
        /// Delete a specific entity.
        /// </summary>
        /// <param name="email">Email of the entity to delete.</param>
        /// <returns>A <see cref="HttpStatusCode"/> indicating the status of the deletion.</returns>
        /// <exception cref="InvalidOperationException">When the method is used when no user is logged in.</exception>
        /// <seealso cref="Client{TResponse, TRequest}.DeleteAsync(Guid)"/>
        public async Task<HttpStatusCode> DeleteAsync(string email)
        {
            // Fetch the logged in user
            ClaimsPrincipal user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

            // Call the APIs with the permissions granted to the user
            HttpResponseMessage response = await downstreamApi.CallApiForUserAsync(
                serviceName: InferServiceName(user),
                downstreamApiOptionsOverride: options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Delete);
                    options.RelativePath = $"{Endpoint}/{email}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = InferAuthenticationScheme(user);
                },
                user: user);

            InvokeResponseHandler(response);

            return response.StatusCode;
        }
    }
}