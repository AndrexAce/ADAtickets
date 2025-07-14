﻿/*
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
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;

namespace ADAtickets.Web.Components.Utilities
{
    /// <summary>
    /// Provides utility methods for creating dialog parameters and handling dialog-related actions.
    /// </summary>
    public static class Dialogs
    {
        /// <summary>
        /// Creates a set of dialog parameters to configure its appearance and behavior.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="okButtonText">The text of the confirm button.</param>
        /// <param name="cancelButtonText">The text of the cancel button.</param>
        /// <returns></returns>
        public static DialogParameters DialogParameters(string title, string okButtonText, string cancelButtonText) => new()
        {
            Title = title,
            PrimaryAction = okButtonText,
            SecondaryAction = cancelButtonText,
            TrapFocus = true,
            PreventScroll = true,
            PreventDismissOnOverlayClick = true
        };

        /// <summary>
        /// Contains data to show in the <c>SimpleDialog.razor</c> component.
        /// </summary>
        public record SimpleDialogContent
        {
            /// <summary>
            /// Message to show in the dialog.
            /// </summary>
            public required string Message { get; set; }

            /// <summary>
            /// Function to execute when pressing the confirm button.
            /// </summary>
            /// <remarks>It accepts a <see cref="MouseEventArgs"/> argument and returns a <see cref="Task"/>.</remarks>
            public required Func<MouseEventArgs, Task> ConfirmAction { get; set; }

            /// <summary>
            /// Icon to show in the confirm button.
            /// </summary>
            public required Icon ConfirmButtonIcon { get; set; }
        }
    }
}
