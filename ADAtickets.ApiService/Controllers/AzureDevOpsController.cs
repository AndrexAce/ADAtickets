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
using ADAtickets.ApiService.Configs;
using ADAtickets.ApiService.Repositories;
using ADAtickets.Shared.Constants;
using ADAtickets.Shared.Dtos;
using ADAtickets.Shared.Models;
using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.WebApi;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using Controller = ADAtickets.Shared.Constants.Controller;

namespace ADAtickets.ApiService.Controllers
{
    /// <summary>
    /// Web API controller managing requests involving Azure DevOps.
    /// </summary>
    /// <param name="userRepository">Object defining the operations allowed on the entity type.</param>
    /// <param name="configuration">Configuration object containing the application settings.</param>
    [Route($"v{Service.APIVersion}/{Controller.AzureDevOps}")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [FormatFilter]
    [ApiConventionType(typeof(ADAticketsApiConventions))]
    public sealed class AzureDevOpsController(IUserRepository userRepository, IConfiguration configuration) : ControllerBase
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IConfiguration _configuration = configuration;

        /// <summary>
        /// Determines if a specific <see cref="User"/> entity has access to the Azure DevOps organization.
        /// </summary>
        /// <param name="email">Email of the <see cref="User"/> entity to check.</param>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response.</returns>
        /// <response code="200">The entity was found.</response>
        /// <response code="400">The provided email was not valid.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="404">The entity with the given email didn't exist.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpGet("{email}/has-access")]
        [Authorize(Policy = Policy.OperatorOrAdmin)]
        [RequiredScope(Scope.Read)]
        public async Task<ActionResult<ValueWrapper<bool>>> GetUserDevOpsAccess(string email)
        {
            // Check if the entity with the given key exists.
            if (!(await _userRepository.GetUsersByAsync([new KeyValuePair<string, string>("Email", email)])).Any())
            {
                return NotFound();
            }

            // Insert the entity data into a new DTO and send it to the client.
            return Ok(await CheckAzureDevOpsAccessAsync(email));
        }

        private async Task<VssConnection> ConnectToAzureDevOpsAsync()
        {
            // Read AzureDevOps settings from configuration
            var tenantId = _configuration["AzureDevOps:TenantId"]!;
            var clientId = _configuration["AzureDevOps:ClientId"]!;
            var certPath = _configuration["AzureDevOps:ClientCertificates:0:CertificateDiskPath"]!;
            var certPassword = _configuration["AzureDevOps:ClientCertificates:0:CertificatePassword"]!;
            var scope = _configuration["AzureDevOpsAPI:Scopes:0"]!;
            var devOpsUri = _configuration["AzureDevOpsAPI:BaseUrl"]!;

            // Create the credentials
            var credentials = new ClientCertificateCredential(
                tenantId: tenantId,
                clientId: clientId,
                clientCertificate: X509CertificateLoader.LoadPkcs12FromFile(certPath, certPassword)
            );

            // Get the Entra access token
            var accessToken = await credentials.GetTokenAsync(
                new Azure.Core.TokenRequestContext([scope])
            );

            var vssToken = new VssAadToken("Bearer", accessToken.Token);
            var vssCredential = new VssAadCredential(vssToken);

            // Connect to Azure DevOps
            return new(new Uri(devOpsUri), vssCredential);
        }

        private async Task<ValueWrapper<bool>> CheckAzureDevOpsAccessAsync(string email)
        {
            var connection = await ConnectToAzureDevOpsAsync();

            var identityClient = await connection.GetClientAsync<IdentityHttpClient>();

            // Queries the list of users who belong to the Azure DevOps organization
            var identities = await identityClient.ReadIdentitiesAsync(
                IdentitySearchFilter.MailAddress,
                email,
                queryMembership: QueryMembership.None);

            return new(identities?.Any() ?? false);
        }
    }
}
