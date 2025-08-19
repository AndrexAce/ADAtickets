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

using Microsoft.AspNetCore.SignalR.Client;

namespace ADAtickets.Web.Components.Utilities;

/// <summary>
///     Service managing real-time communication with the APIs.
/// </summary>
internal sealed class SignalRService : IAsyncDisposable
{
    private HubConnection? hubConnection = null;

    /// <summary>
    ///     Starts the SignalR connection to the specified hub URL.
    /// </summary>
    /// <param name="hubUrl">URL of the hub to connect to.</param>
    /// <param name="handlersRegistration">Function where all the method handlers called by the hubs are registered.</param>
    /// <returns>A <see cref="Task"/> running the operation.</returns>
    public async Task StartAsync(string hubUrl, Action handlersRegistration)
    {
        hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        handlersRegistration.Invoke();

        await hubConnection.StartAsync();
    }

    /// <summary>
    ///     Stops the SignalR connection if it is running and disposes of it.
    /// </summary>
    /// <returns>A <see cref="Task"/> running the operation.</returns>
    public async Task StopAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.StopAsync();
            await hubConnection.DisposeAsync();
            hubConnection = null;
        }
    }

    /// <summary>
    ///     Registers a handler for a specific method called by the SignalR hub.
    /// </summary>
    /// <param name="methodName">Name of the method called by the server.</param>
    /// <param name="handler">Asynchronous function handling the call.</param>
    public void On(string methodName, Func<Task> handler)
    {
        hubConnection?.On(methodName, handler);
    }

    /// <summary>
    ///     Registers a handler for a specific method called by the SignalR hub.
    /// </summary>
    /// <typeparam name="T">Type of the argument passed by the server.</typeparam>
    /// <param name="methodName">Name of the method called by the server.</param>
    /// <param name="handler">Asynchronous function handling the call.</param>
    public void On<T>(string methodName, Func<T, Task> handler)
    {
        hubConnection?.On(methodName, handler);
    }

    /// <summary>
    ///     Invokes a method on the API through the SignalR coonection.
    /// </summary>
    /// <param name="methodName">Name of the method to be called on the server.</param>
    /// <param name="arg">Argument to pass to the function call.</param>
    /// <returns>A <see cref="Task"/> running the operation.</returns>
    public async Task SendAsync(string methodName, object arg)
    {
        if (hubConnection is not null)
        {
            await hubConnection.SendAsync(methodName, arg);
        }
    }

    /// <summary>
    ///     Disposes the unmanaged connection.
    /// </summary>
    /// <returns>The <see cref="ValueTask"> of the asynchronous operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (hubConnection is not null)
        {
            await hubConnection.DisposeAsync();
            hubConnection = null;
        }
    }
}