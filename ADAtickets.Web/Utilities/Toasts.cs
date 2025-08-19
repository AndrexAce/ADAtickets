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

using Microsoft.FluentUI.AspNetCore.Components;

namespace ADAtickets.Web.Utilities;

/// <summary>
///     Provides utility methods for creating toast parameters and handling toast-related actions.
/// </summary>
public static class Toasts
{
    /// <summary>
    ///     Creates a set of toast parameters for an error toast to configure its appearance and behavior.
    /// </summary>
    /// <param name="exception">The exception which message will be shown.</param>
    /// <param name="title">The title of the toast.</param>
    /// <param name="details">The message of the toast.</param>
    /// <returns>A <see cref="ToastParameters{T}" /> object with the specified title and details.</returns>
    public static ToastParameters<CommunicationToastContent> ErrorToastParameters(Exception exception, string title,
        string details)
    {
        return new ToastParameters<CommunicationToastContent>
        {
            Intent = ToastIntent.Error,
            Title = title,
            Content = new CommunicationToastContent
            {
                Details = details,
                Subtitle = exception.Message
            }
        };
    }

    /// <summary>
    ///     Creates a <see cref="ToastParameters{T}" /> instance configured for a success notification.
    /// </summary>
    /// <param name="title">The title of the toast notification.</param>
    /// <param name="details">The detailed message content of the toast notification.</param>
    /// <returns>A <see cref="ToastParameters{T}" /> object with the specified title and details.</returns>
    public static ToastParameters<CommunicationToastContent> ConfirmToastParameters(string title, string details)
    {
        return new ToastParameters<CommunicationToastContent>
        {
            Intent = ToastIntent.Success,
            Title = title,
            Content = new CommunicationToastContent
            {
                Details = details
            }
        };
    }


    /// <summary>
    ///     Creates a set of toast parameters for a progress toast to configure its appearance and behavior.
    /// </summary>
    /// <param name="id">
    ///     The ID of the toast, used to update it with
    ///     <see cref="IToastService.UpdateToast{TContent}(string, ToastParameters{TContent})" />.
    /// </param>
    /// <param name="title">The title of the toast.</param>
    /// <param name="details">The message of the toast.</param>
    /// <returns>A <see cref="ToastParameters{T}" /> object with the specified title and details.</returns>
    public static ToastParameters<ProgressToastContent> ProgressToastParameters(string id, string title, string details)
    {
        return new ToastParameters<ProgressToastContent>
        {
            Id = id,
            Intent = ToastIntent.Progress,
            Title = title,
            Content = new ProgressToastContent
            {
                Details = details,
                Progress = 0
            }
        };
    }

    /// <summary>
    ///     Creates a <see cref="ToastParameters{T}" /> instance configured for a warning notification.
    /// </summary>
    /// <param name="title">The title of the toast notification.</param>
    /// <param name="details">The detailed message content of the toast notification.</param>
    /// <returns>A <see cref="ToastParameters{T}" /> object with the specified title and details.</returns>
    public static ToastParameters<CommunicationToastContent> WarningToastParameters(string title, string details)
    {
        return new ToastParameters<CommunicationToastContent>
        {
            Intent = ToastIntent.Warning,
            Title = title,
            Content = new CommunicationToastContent
            {
                Details = details
            }
        };
    }
}