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
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Identity.Client;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System.Dynamic;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using ADAticketsUser = ADAtickets.Shared.Models.User;
using Controller = ADAtickets.Shared.Constants.Controller;

namespace ADAtickets.ApiService.Controllers;

/// <summary>
///     Web API controller managing requests involving Azure DevOps.
/// </summary>
/// <param name="userRepository">Object defining the operations allowed on the <see cref="User" /> entity type.</param>
/// <param name="platformRepository">Object defining the operations allowed on the <see cref="Platform" /> entity type.</param>
/// <param name="configuration">Configuration object containing the application settings.</param>
[Route($"v{Service.APIVersion}/{Controller.AzureDevOps}")]
[ApiController]
[Consumes(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
[Produces(MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml)]
[FormatFilter]
[ApiConventionType(typeof(ApiConventions))]
public sealed class AzureDevOpsController(
    IConfiguration configuration,
    IUserRepository userRepository,
    IPlatformRepository platformRepository
    ) : ControllerBase
{
    /// <summary>
    ///     Determines if a specific <see cref="User" /> entity with <paramref name="email" /> has access to the Azure DevOps
    ///     organization.
    /// </summary>
    /// <param name="email">Email of the <see cref="User" /> entity to check.</param>
    /// <returns>A <see cref="Task" /> returning an <see cref="ActionResult" />, which wraps the server response.</returns>
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
        if (!(await userRepository.GetUsersByAsync(new Dictionary<string, string> { { nameof(ADAticketsUser.Email), email } })).Any())
            return NotFound();

        // Insert the entity data into a new DTO and send it to the client.
        return Ok(await CheckAzureDevOpsAccessAsync(email));
    }

    /// <summary>
    ///     Fetches all the platforms available in the Azure DevOps organization.
    /// </summary>
    /// <returns>
    ///     A <see cref="Task" /> returning an <see cref="ActionResult" />, which wraps the server response and the list
    ///     of entities.
    /// </returns>
    /// <response code="200">The entity was found.</response>
    /// <response code="401">The client was not authenticated.</response>
    /// <response code="403">The client was authenticated but had not enough privileges.</response>
    /// <response code="406">The client asked for an unsupported response format.</response>
    [HttpGet("projects")]
    [Authorize(Policy = Policy.OperatorOrAdmin)]
    [RequiredScope(Scope.Read)]
    public async Task<ActionResult<IEnumerable<PlatformResponseDto>>> GetAllPlatforms()
    {
        var devOpsPlatforms = await GetAzureDevOpsPlatformsAsync();

        return Ok(devOpsPlatforms);
    }

    private async Task<VssConnection> ConnectToAzureDevOpsAsync()
    {
        // Read AzureDevOps settings from configuration
        var tenantId = configuration["AzureDevOps:TenantId"]!;
        var clientId = configuration["AzureDevOps:ClientId"]!;
        var certPath = configuration["AzureDevOps:ClientCertificates:0:CertificateDiskPath"]!;
        var certPassword = configuration["AzureDevOps:ClientCertificates:0:CertificatePassword"]!;
        var scope = configuration["AzureDevOpsAPI:Scopes:0"]!;
        var devOpsUri = configuration["AzureDevOpsAPI:BaseUrl"]!;

        // Create the credentials
        ClientCertificateCredential credentials = new(
            tenantId,
            clientId,
            X509CertificateLoader.LoadPkcs12FromFile(certPath, certPassword)
        );

        // Get the Entra access token
        var accessToken = await credentials.GetTokenAsync(
            new TokenRequestContext([scope])
        );

        VssAadToken vssToken = new("Bearer", accessToken.Token);
        VssAadCredential vssCredential = new(vssToken);

        // Connect to Azure DevOps
        return new VssConnection(new Uri(devOpsUri), vssCredential);
    }

    private async Task<ValueWrapper<bool>> CheckAzureDevOpsAccessAsync(string email)
    {
        var connection = await ConnectToAzureDevOpsAsync();

        var identityClient = await connection.GetClientAsync<IdentityHttpClient>();

        // Queries the list of users who belong to the Azure DevOps organization
        var identities = await identityClient.ReadIdentitiesAsync(
            IdentitySearchFilter.MailAddress,
            email);

        return new ValueWrapper<bool>(identities?.Any() ?? false);
    }

    private async Task<IEnumerable<PlatformResponseDto>> GetAzureDevOpsPlatformsAsync()
    {
        var connection = await ConnectToAzureDevOpsAsync();

        var projectClient = await connection.GetClientAsync<ProjectHttpClient>();

        // Fetches all projects in the Azure DevOps organization
        IEnumerable<TeamProjectReference> projects = await projectClient.GetProjects();

        return projects.Select(project => new PlatformResponseDto
        {
            Id = project.Id,
            Name = project.Name,
            RepositoryUrl = project.Url
        });
    }

    internal async Task<int?> CreateAzureDevOpsWorkItemAsync(Ticket ticket)
    {
        var platform = await platformRepository.GetPlatformByIdAsync(ticket.PlatformId);
        var platformName = platform?.Name;

        if (platformName is null)
        {
            return null;
        }

        var connection = await ConnectToAzureDevOpsAsync();

        try
        {
            // Get the client and create the work item
            var workItemClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

            var creationPatchDocument = new JsonPatchDocument
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
                    Value = 3 - (int)ticket.Priority
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
                    Value = "To Do"
                },
            };

            var workItem = await workItemClient.CreateWorkItemAsync(creationPatchDocument, platformName,
                ticket.Type == TicketType.Bug ? "Issue" : "Task");

            // Update the work item with the assigned user (can't be done in creation)

            if (workItem is null || !workItem.Id.HasValue)
            {
                return null;
            }
            else if (ticket.OperatorUser is null)
            {
                return workItem.Id;
            }

            var updatePatchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.AssignedTo",
                    Value = $"{ticket.OperatorUser.Username}<{ticket.OperatorUser.Email}>"
                }
            };

            workItem = await workItemClient.UpdateWorkItemAsync(updatePatchDocument, workItem.Id.Value);

            return workItem.Id;
        }
        catch
        {
            return null;
        }
    }

    internal async Task UpdateAzureDevOpsWorkItemAsync(Ticket ticket)
    {
        if (ticket.WorkItemId == 0)
        {
            return;
        }

        var platform = await platformRepository.GetPlatformByIdAsync(ticket.PlatformId);
        var platformName = platform?.Name;

        if (platformName is null)
        {
            return;
        }

        var connection = await ConnectToAzureDevOpsAsync();

        try
        {
            // Get the client and update the work item
            var workItemClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

            var updatePatchDocument = new JsonPatchDocument
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
                    Value = 3 - (int)ticket.Priority
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.State",
                    Value = ticket.Status switch
                    {
                        Status.Unassigned => "To Do",
                        Status.WaitingUser or Status.WaitingOperator => "Doing",
                        Status.Closed => "Done",
                        _ => "To Do"
                    }
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.WorkItemType",
                    Value = ticket.Type == TicketType.Bug ? "Issue" : "Task"
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.TeamProject",
                    Value = ticket.Platform.Name
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.AreaPath",
                    Value = ticket.Platform.Name
                },
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.IterationPath",
                    Value = ticket.Platform.Name
                }
            };

            await workItemClient.UpdateWorkItemAsync(updatePatchDocument, ticket.WorkItemId);
        }
        catch
        {
            // Do nothing.
        }
    }

    internal async Task UpdateOperatorAzureDevOpsWorkItemAsync(Ticket ticket)
    {
        if (ticket.WorkItemId == 0)
        {
            return;
        }

        var connection = await ConnectToAzureDevOpsAsync();

        try
        {
            // Get the client and update the work item
            var workItemClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

            if (!ticket.OperatorUserId.HasValue)
            {
                // If the operator user is null, we remove the AssignedTo field
                var updatePatchDocument = new JsonPatchDocument
                {
                    new JsonPatchOperation
                    {
                        Operation = Operation.Remove,
                        Path = "/fields/System.AssignedTo"
                    }
                };

                await workItemClient.UpdateWorkItemAsync(updatePatchDocument, ticket.WorkItemId);
            }
            else
            {
                // Fetch the assigned operator for the ticket
                var assignedOperator = await userRepository.GetUserByIdAsync(ticket.OperatorUserId.Value);

                if (assignedOperator is null)
                {
                    return;
                }

                var updatePatchDocument = new JsonPatchDocument
                {
                    new JsonPatchOperation
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.AssignedTo",
                        Value = $"{assignedOperator.Username}<{assignedOperator.Email}>"
                    }
                };

                await workItemClient.UpdateWorkItemAsync(updatePatchDocument, ticket.WorkItemId);
            }
        }
        catch
        {
            // Do nothing.
        }
    }

    internal async Task DeleteAzureDevOpsWorkItemAsync(int workItemId)
    {
        if (workItemId == 0)
        {
            return;
        }

        var connection = await ConnectToAzureDevOpsAsync();

        try
        {
            // Get the client and create the work item
            var workItemClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

            await workItemClient.DeleteWorkItemAsync(workItemId);
        }
        catch
        {
            // Do nothing.
        }
    }

    internal async Task CreateCommentAzureDevOpsWorkItemAsync(int workItemId, string platform, string authorName, string authorEmail, string message)
    {
        if (workItemId == 0)
        {
            return;
        }

        var connection = await ConnectToAzureDevOpsAsync();

        try
        {
            // Get the client and create the comment
            var workItemClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

            var createRequest = new CommentCreate() { Text = $"<b>{authorName} <{authorEmail}>:</b> {message}" };

            await workItemClient.AddWorkItemCommentAsync(createRequest, platform, workItemId, CommentFormat.Html);
        }
        catch
        {
            // Do nothing.
        }
    }

    internal async Task CreateAttachmentAzureDevOpsWorkItemAsync(int workItemId, string attachmentPath, string platformName)
    {
        if (workItemId == 0)
        {
            return;
        }

        var connection = await ConnectToAzureDevOpsAsync();

        try
        {
            // Get the client and create the comment
            var workItemClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

            using var stream = new StreamReader(attachmentPath);

            var attachment = await workItemClient.CreateAttachmentAsync(stream.BaseStream, platformName, Path.GetFileName(attachmentPath), "Simple", platformName);

            // If the attachment was created, add it to the work item
            var attachmentValue = new ExpandoObject();
            attachmentValue.TryAdd("rel", "AttachedFile");
            attachmentValue.TryAdd("url", attachment.Url);

            var updatePatchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = attachmentValue
                }
            };

            await workItemClient.UpdateWorkItemAsync(updatePatchDocument, workItemId);
        }
        catch
        {
            // Do nothing.
        }
    }

    internal async Task<Comment?> GetLastCommentAzureDevOpsWorkItemAsync(int workItemId, string platformName)
    {
        if (workItemId == 0)
        {
            return null;
        }

        var connection = await ConnectToAzureDevOpsAsync();

        try
        {
            // Get the client and get the comments
            var workItemClient = await connection.GetClientAsync<WorkItemTrackingHttpClient>();

            var comments = await workItemClient.GetCommentsAsync(platformName, workItemId);

            return comments.Comments.OrderByDescending(c => c.CreatedDate).FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }
}