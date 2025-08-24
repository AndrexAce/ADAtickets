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
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace ADAtickets.Shared.Dtos.Responses;

public sealed record AzureDevOpsWebHookResponseDto
{
    public Guid SubscriptionId { get; init; } = Guid.Empty;

    public int NotificationId { get; init; } = 0;

    public Guid Id { get; init; } = Guid.Empty;

    public string EventType { get; init; } = string.Empty;

    public string PublisherId { get; init; } = string.Empty;

    public sealed record AzureDevOpsMessage
    {
        public string Text { get; init; } = string.Empty;

        public string Html { get; init; } = string.Empty;

        public string Markdown { get; init; } = string.Empty;
    }

    public AzureDevOpsMessage Message { get; init; } = new AzureDevOpsMessage();

    public AzureDevOpsMessage DetailedMessage { get; init; } = new AzureDevOpsMessage();

    public WorkItem Resource { get; init; } = new WorkItem();
}
