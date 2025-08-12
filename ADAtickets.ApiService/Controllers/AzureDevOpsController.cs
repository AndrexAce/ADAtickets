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
using ADAtickets.ApiService.Configs;
using ADAtickets.ApiService.Repositories;
using ADAtickets.Shared.Constants;
using ADAtickets.Shared.Dtos.Responses;
using ADAtickets.Shared.Models;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
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
    /// <param name="platformRepository">Object defining the operations allowed on the <see cref="Platform"/> entity type.</param>
    [Route($"v{Service.APIVersion}/{Controller.AzureDevOps}")]
    [ApiController]
    [Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
    [FormatFilter]
    [ApiConventionType(typeof(ApiConventions))]
    public sealed class AzureDevOpsController(IUserRepository userRepository, IConfiguration configuration, IPlatformRepository platformRepository) : ControllerBase
    {
        /// <summary>
        /// Determines if a specific <see cref="User"/> entity with <paramref name="email"/> has access to the Azure DevOps organization.
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
            if (!(await userRepository.GetUsersByAsync([new KeyValuePair<string, string>("Email", email)])).Any())
            {
                return NotFound();
            }

            // Insert the entity data into a new DTO and send it to the client.
            return Ok(await CheckAzureDevOpsAccessAsync(email));
        }

        /// <summary>
        /// Fetches all the platforms available in the Azure DevOps organization.
        /// </summary>
        /// <returns>A <see cref="Task"/> returning an <see cref="ActionResult"/>, which wraps the server response and the list of entities.</returns>
        /// <response code="200">The entity was found.</response>
        /// <response code="401">The client was not authenticated.</response>
        /// <response code="403">The client was authenticated but had not enough privileges.</response>
        /// <response code="406">The client asked for an unsupported response format.</response>
        [HttpGet("projects")]
        [Authorize(Policy = Policy.OperatorOrAdmin)]
        [RequiredScope(Scope.Read)]
        public async Task<ActionResult<IEnumerable<PlatformResponseDto>>> GetAllPlatformNamesAsync()
        {
            IEnumerable<PlatformResponseDto> devOpsPlatforms = await GetAzureDevOpsPlatformsAsync();

            return Ok(devOpsPlatforms);
        }

        private async Task<VssConnection> ConnectToAzureDevOpsAsync()
        {
            // Read AzureDevOps settings from configuration
            string tenantId = configuration["AzureDevOps:TenantId"]!;
            string clientId = configuration["AzureDevOps:ClientId"]!;
            string certPath = configuration["AzureDevOps:ClientCertificates:0:CertificateDiskPath"]!;
            string certPassword = configuration["AzureDevOps:ClientCertificates:0:CertificatePassword"]!;
            string scope = configuration["AzureDevOpsAPI:Scopes:0"]!;
            string devOpsUri = configuration["AzureDevOpsAPI:BaseUrl"]!;

            // Create the credentials
            ClientCertificateCredential credentials = new(
                tenantId: tenantId,
                clientId: clientId,
                clientCertificate: X509CertificateLoader.LoadPkcs12FromFile(certPath, certPassword)
            );

            // Get the Entra access token
            AccessToken accessToken = await credentials.GetTokenAsync(
                new TokenRequestContext([scope])
            );

            VssAadToken vssToken = new("Bearer", accessToken.Token);
            VssAadCredential vssCredential = new(vssToken);

            // Connect to Azure DevOps
            return new(new Uri(devOpsUri), vssCredential);
        }

        private async Task<ValueWrapper<bool>> CheckAzureDevOpsAccessAsync(string email)
        {
            VssConnection connection = await ConnectToAzureDevOpsAsync();

            IdentityHttpClient identityClient = await connection.GetClientAsync<IdentityHttpClient>();

            // Queries the list of users who belong to the Azure DevOps organization
            IdentitiesCollection identities = await identityClient.ReadIdentitiesAsync(
                IdentitySearchFilter.MailAddress,
                email,
                queryMembership: QueryMembership.None);

            return new(identities?.Any() ?? false);
        }

        private async Task<IEnumerable<PlatformResponseDto>> GetAzureDevOpsPlatformsAsync()
        {
            VssConnection connection = await ConnectToAzureDevOpsAsync();

            ProjectHttpClient projectClient = await connection.GetClientAsync<ProjectHttpClient>();

            // Fetches all projects in the Azure DevOps organization
            IEnumerable<TeamProjectReference> projects = await projectClient.GetProjects();

            return projects.Select(project => new PlatformResponseDto
            {
                Id = project.Id,
                Name = project.Name,
                RepositoryUrl = project.Url
            });
        }

        internal async Task<ValueWrapper<bool>> CreateAzureDevOpsWorkItem(Ticket ticket, Guid platformId)
        {
            var platform = await platformRepository.GetPlatformByIdAsync(platformId);
            var platformName = platform?.Name;

            if (platformName is not null)
            {
                VssConnection connection = await ConnectToAzureDevOpsAsync();

                try
                {
                    WorkItemTrackingHttpClient workItemClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

                    var patchDocument = new JsonPatchDocument
                    {
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = "/fields/System.Title",
                            Value = ticket.Title
                        },
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = "/fields/System.Description",
                            Value = ticket.Description
                        },
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = "/fields/Microsoft.VSTS.Common.Priority",
                            Value = (int)ticket.Priority
                        },
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = "/fields/System.CreatedDate",
                            Value = ticket.CreationDateTime
                        },
                        new JsonPatchOperation
                        {
                            Operation = Operation.Add,
                            Path = "/fields/System.State",
                            Value = "New"
                        }
                    };

                    await workItemClient.CreateWorkItemAsync(patchDocument, platformName, ticket.Type == TicketType.Bug ? "Issue" : "Task");
                }
                catch
                {
                    return new(false);
                }
            }

            return new(true);
        }
    }
}