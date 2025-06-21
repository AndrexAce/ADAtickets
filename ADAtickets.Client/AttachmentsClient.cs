using ADAtickets.Shared.Constants;
using ADAtickets.Shared.Dtos.Requests;
using ADAtickets.Shared.Dtos.Responses;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Abstractions;
using System.Net;
using System.Net.Http.Json;

namespace ADAtickets.Client
{
    /// <summary>
    /// Provides methods to interact with ADAtickets AttachmentsController.
    /// </summary>
    /// <param name="authenticationStateProvider">The provider of the signed in user's data.</param>
    /// <param name="downstreamApi">The downstream API service to make requests.</param>
    public sealed class AttachmentsClient(AuthenticationStateProvider authenticationStateProvider, IDownstreamApi downstreamApi)
        : Client<AttachmentResponseDto, AttachmentRequestDto>(authenticationStateProvider, downstreamApi)
    {
        private const string endpoint = $"v{Service.APIVersion}/{Controller.Attachments}";

        /// <summary>
        /// Fetches all attachments or those matching the given filters.
        /// </summary>
        public async Task<(HttpStatusCode, IEnumerable<AttachmentResponseDto>? )> GetAttachmentsAsync(IEnumerable<KeyValuePair<string, string>>? filters = null)
        {
            var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
            var query = filters != null
                ? "?" + string.Join("&", filters.Select(f => $"{Uri.EscapeDataString(f.Key)}={Uri.EscapeDataString(f.Value)}"))
                : string.Empty;

            var response = await _downstreamApi.CallApiForUserAsync(
                serviceName: Service.API,
                options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Get);
                    options.RelativePath = $"{endpoint}{query}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = Scheme.OpenIdConnectDefault;
                }, user);

            return (response.StatusCode, response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<IEnumerable<AttachmentResponseDto>>()
                : null);
        }

        /// <summary>
        /// Fetches a specific attachment by id.
        /// </summary>
        public async Task<(HttpStatusCode, AttachmentResponseDto?)> GetAttachmentAsync(Guid id)
        {
            var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
            var response = await _downstreamApi.CallApiForUserAsync(
                serviceName: Service.API,
                options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Get);
                    options.RelativePath = $"{endpoint}/{id}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = Scheme.OpenIdConnectDefault;
                }, user);

            return (response.StatusCode, response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<AttachmentResponseDto>()
                : null);
        }

        /// <summary>
        /// Creates a new attachment.
        /// </summary>
        public async Task<(HttpStatusCode, AttachmentResponseDto?)> PostAttachmentAsync(AttachmentRequestDto attachmentDto)
        {
            var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
            var response = await _downstreamApi.CallApiForUserAsync(
                serviceName: Service.API,
                options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Post);
                    options.RelativePath = endpoint;
                    options.JsonBody = attachmentDto;
                    options.AcquireTokenOptions.AuthenticationOptionsName = Scheme.OpenIdConnectDefault;
                }, user);

            return (response.StatusCode, response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<AttachmentResponseDto>()
                : null);
        }

        /// <summary>
        /// Updates an existing attachment or creates it if it does not exist.
        /// </summary>
        public async Task<(HttpStatusCode, AttachmentResponseDto?)> PutAttachmentAsync(Guid id, AttachmentRequestDto attachmentDto)
        {
            var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
            var response = await _downstreamApi.CallApiForUserAsync(
                serviceName: Service.API,
                options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Put);
                    options.RelativePath = $"{endpoint}/{id}";
                    options.JsonBody = attachmentDto;
                    options.AcquireTokenOptions.AuthenticationOptionsName = Scheme.OpenIdConnectDefault;
                }, user);

            // 201: created, 204: no content, 409: conflict, etc.
            if (response.StatusCode == HttpStatusCode.NoContent)
                return (response.StatusCode, null);

            return (response.StatusCode, response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<AttachmentResponseDto>()
                : null);
        }

        /// <summary>
        /// Deletes an attachment by id.
        /// </summary>
        public async Task<HttpStatusCode> DeleteAttachmentAsync(Guid id)
        {
            var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
            var response = await _downstreamApi.CallApiForUserAsync(
                serviceName: Service.API,
                options =>
                {
                    options.HttpMethod = nameof(HttpMethod.Delete);
                    options.RelativePath = $"{endpoint}/{id}";
                    options.AcquireTokenOptions.AuthenticationOptionsName = Scheme.OpenIdConnectDefault;
                }, user);

            return response.StatusCode;
        }
    }
}