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

using ADAtickets.Shared.Dtos.Responses;
using ADAtickets.Shared.Models;
using Microsoft.FluentUI.AspNetCore.Components;

namespace ADAtickets.Web.Components.Utilities;

/// <summary>
///     Provides utility methods for creating dialog parameters and handling dialog-related actions.
/// </summary>
public static class Dialogs
{
    /// <summary>
    ///     Creates a set of dialog parameters to configure its appearance and behavior.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="okButtonText">The text of the confirm button.</param>
    /// <param name="cancelButtonText">The text of the cancel button.</param>
    /// <returns>A <see cref="DialogParameters" /> object with the specified title, button texts, and behavior settings.</returns>
    public static DialogParameters ConfirmDialogParameters(string title, string okButtonText, string cancelButtonText)
    {
        return new DialogParameters
        {
            DialogType = DialogType.MessageBox,
            Title = title,
            PrimaryAction = okButtonText,
            SecondaryAction = cancelButtonText,
            TrapFocus = true,
            PreventScroll = true,
            PreventDismissOnOverlayClick = true
        };
    }

    /// <summary>
    ///     Creates a set of dialog parameters to configure a panel dialog.
    /// </summary>
    /// <returns>A <see cref="DialogParameters" /> object with the specified behaviour settings.</returns>
    public static DialogParameters PanelParameters()
    {
        return new DialogParameters
        {
            DialogType = DialogType.Panel,
            Alignment = HorizontalAlignment.Right,
            TrapFocus = true,
            PreventScroll = true,
            PreventDismissOnOverlayClick = true
        };
    }

    /// <summary>
    ///     Contains data to show in the <c>SimpleDialog.razor</c> component.
    /// </summary>
    public record SimpleDialogContent
    {
        /// <summary>
        ///     Message to show in the dialog.
        /// </summary>
        public required string Message { get; init; }

        /// <summary>
        ///     Function to execute when pressing the confirm button.
        /// </summary>
        /// <remarks>It accepts no parameters and returns a <see cref="Task" />.</remarks>
        public required Func<Task> ConfirmAction { get; init; }

        /// <summary>
        ///     Icon to show in the confirm button.
        /// </summary>
        public required Icon ConfirmButtonIcon { get; init; }
    }

    /// <summary>
    ///     Contains data to show in the <c>TicketDialog.razor</c> component.
    /// </summary>
    public record TicketDialogContent
    {
        /// <summary>
        ///     Whether the dialog is in edit mode or not.
        /// </summary>
        public required bool IsEdit { get; init; }

        /// <summary>
        ///     Whether the person opening the dialog is a <see cref="UserType.User" />.
        /// </summary>
        public required bool IsUser { get; init; }

        /// <summary>
        ///     Initial data to show in the ticket dialog (optional).
        /// </summary>
        public TicketResponseDto? InitialTicketData { get; init; }
    }
}