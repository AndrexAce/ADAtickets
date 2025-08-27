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
using Newtonsoft.Json;
using System.Net;
using System.Net.Mime;

namespace ADAtickets.Client;

/// <summary>
///     Provides methods to interact with ADAtickets UsersController.
/// </summary>
/// <param name="authenticationStateProvider">The provider of the signed in user's data.</param>
/// <param name="downstreamApi">The downstream API service to make requests.</param>
/// <param name="configuration">The configuration settings.</param>
public sealed class UsersClient(
    AuthenticationStateProvider authenticationStateProvider,
    IDownstreamApi downstreamApi,
    IConfiguration configuration)
    : Client<UserResponseDto, UserRequestDto>(authenticationStateProvider, downstreamApi, configuration)
{
    /// <inheritdoc cref="Client{TResponse, TRequest}.ControllerName" />
    protected override string ControllerName => Controller.Users;

    /// <summary>
    ///     Fetch a specific entity.
    /// </summary>
    /// <param name="email">Email of the entity to fetch.</param>
    /// <returns>The requested entity.</returns>
    /// <exception cref="InvalidOperationException">When the method is used when no user is logged in.</exception>
    /// <exception cref="HttpRequestException">When the API call fails.</exception>
    /// <exception cref="JsonException">When the JSON response cannot be parsed.</exception>
    /// <seealso cref="Client{TResponse, TRequest}.GetAsync(Guid)" />
    public async Task<UserResponseDto> GetAsync(string email)
    {
        // Fetch the logged in user
        var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

        // Call the APIs with the permissions granted to the user
        var response = await downstreamApi.CallApiForUserAsync(
            InferServiceName(user),
            options =>
            {
                options.HttpMethod = nameof(HttpMethod.Get);
                options.RelativePath = $"{Endpoint}/{email}";
                options.AcquireTokenOptions.AuthenticationOptionsName = InferAuthenticationScheme(user);
            },
            user);

        if (response.StatusCode is HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserResponseDto>(content, JsonSettings) ??
                   throw new JsonException(JsonExceptionMessage);
        }

        throw new HttpRequestException(response.ReasonPhrase, null, response.StatusCode);
    }

    /// <summary>
    ///     Update a specific entity.
    /// </summary>
    /// <param name="email">Email of the entity to update.</param>
    /// <param name="entity">Object containing the new values the fields should be updated to.</param>
    /// <returns>The entity if it was created; otherwise, <see langword="null" /> if the entity was updated.</returns>
    /// <exception cref="InvalidOperationException">When the method is used when no user is logged in.</exception>
    /// <exception cref="HttpRequestException">When the API call fails.</exception>
    /// <exception cref="JsonException">When the JSON response cannot be parsed.</exception>
    /// <seealso cref="Client{TResponse, TRequest}.PutAsync(Guid, TRequest)" />
    public async Task<UserResponseDto?> PutAsync(string email, UserRequestDto entity)
    {
        // Fetch the logged in user
        var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

        var jsonContent = JsonConvert.SerializeObject(entity, JsonSettings);
        var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json);

        // Call the APIs with the permissions granted to the user
        var response = await downstreamApi.CallApiForUserAsync(
            InferServiceName(user),
            options =>
            {
                options.HttpMethod = nameof(HttpMethod.Put);
                options.ContentType = MediaTypeNames.Application.Json;
                options.RelativePath = $"{Endpoint}/{email}";
                options.AcquireTokenOptions.AuthenticationOptionsName = InferAuthenticationScheme(user);
            },
            user,
            httpContent);

        if (response.StatusCode is HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UserResponseDto>(content, JsonSettings) ??
                   throw new JsonException(JsonExceptionMessage);
        }

        return response.StatusCode is HttpStatusCode.NoContent
            ? null
            : throw new HttpRequestException(response.ReasonPhrase, null, response.StatusCode);
    }

    /// <summary>
    ///     Delete a specific entity.
    /// </summary>
    /// <param name="email">Email of the entity to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">When the method is used when no user is logged in.</exception>
    /// <exception cref="HttpRequestException">When the API call fails.</exception>
    /// <seealso cref="Client{TResponse, TRequest}.DeleteAsync(Guid)" />
    public async Task DeleteAsync(string email)
    {
        // Fetch the logged in user
        var user = (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

        // Call the APIs with the permissions granted to the user
        var response = await downstreamApi.CallApiForUserAsync(
            InferServiceName(user),
            options =>
            {
                options.HttpMethod = nameof(HttpMethod.Delete);
                options.RelativePath = $"{Endpoint}/{email}";
                options.AcquireTokenOptions.AuthenticationOptionsName = InferAuthenticationScheme(user);
            },
            user);

        if (response.StatusCode is not HttpStatusCode.NoContent)
            throw new HttpRequestException(response.ReasonPhrase, null, response.StatusCode);
    }
}