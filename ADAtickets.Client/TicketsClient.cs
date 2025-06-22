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
using Microsoft.Identity.Abstractions;

namespace ADAtickets.Client
{
    /// <summary>
    /// Provides methods to interact with ADAtickets TicketsController.
    /// </summary>
    /// <param name="authenticationStateProvider">The provider of the signed in user's data.</param>
    /// <param name="downstreamApi">The downstream API service to make requests.</param>
    public sealed class TicketsClient(AuthenticationStateProvider authenticationStateProvider, IDownstreamApi downstreamApi)
        : Client<TicketResponseDto, TicketRequestDto>(authenticationStateProvider, downstreamApi)
    {
        /// <inheritdoc cref="Client{TResponse, TRequest}.ControllerName"/>
        protected override string ControllerName => Controller.Tickets;
    }
}