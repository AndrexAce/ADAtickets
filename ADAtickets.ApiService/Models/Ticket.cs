﻿/*
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
using AutoMapper.Configuration.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ADAtickets.ApiService.Models
{
    /// <summary>
    /// Represents a ticket sent by a user to the system.
    /// </summary>
    public sealed class Ticket : EntityBase
    {
        /// <summary>
        /// The unique identifier of the ticket.
        /// </summary>
        [Key]
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The type of user request bound to the ticket.
        /// </summary>
        [Required]
        public TicketType Type { get; set; } = TicketType.BUG;

        /// <summary>
        /// The date and time when the ticket was created.
        /// </summary>
        [Required]
        public DateTimeOffset CreationDateTime { get; } = DateTimeOffset.Now;

        /// <summary>
        /// The title of the ticket, a brief recap of the issue.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The description of the ticket, a detailed description of the issue.
        /// </summary>
        [Required]
        [MaxLength(5000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The urgency of the ticket.
        /// </summary>
        [Required]
        public Priority Priority { get; set; } = Priority.LOW;

        /// <summary>
        /// The status of the ticket.
        /// </summary>
        [Required]
        public Status Status { get; set; } = Status.UNASSIGNED;

        /// <summary>
        /// The id of the work item the ticket is related to in Azure DevOps.
        /// </summary>
        [Required]
        public int WorkItemId { get; set; } = 0;

        /// <summary>
        /// The name of the platform the ticket is related to.
        /// </summary>
        [Required]
        [ForeignKey(nameof(Platform))]
        [MaxLength(4000)]
        public string PlatformName { get; set; } = string.Empty;

        /// <summary>
        /// The platform the ticket is related to.
        /// </summary>
        [Required]
        [Ignore]
        [JsonIgnore]
        public Platform Platform { get; set; } = new Platform();

        /// <summary>
        /// The email of the user who created the ticket.
        /// </summary>
        [Required]
        [ForeignKey(nameof(CreatorUser))]
        [MaxLength(254)]
        [EmailAddress]
        public string CreatorUserEmail { get; set; } = string.Empty;

        /// <summary>
        /// The user who created the ticket.
        /// </summary>
        [Required]
        [Ignore]
        [JsonIgnore]
        public User CreatorUser { get; set; } = new User();

        /// <summary>
        /// The email of the operator assigned to the ticket.
        /// </summary>
        [ForeignKey(nameof(OperatorUser))]
        [MaxLength(254)]
        [EmailAddress]
        public string? OperatorUserEmail { get; set; } = null;

        /// <summary>
        /// The operator assigned to the ticket.
        /// </summary>
        [Ignore]
        [JsonIgnore]
        public User? OperatorUser { get; set; } = null;

        /// <summary>
        /// The collection of edits made to the ticket.
        /// </summary>
        [Required]
        [InverseProperty(nameof(Edit.Ticket))]
        [Ignore]
        [JsonIgnore]
        public ICollection<Edit> Edits { get; } = [];

        /// <summary>
        /// The collection of replies sent to the ticket.
        /// </summary>
        [Required]
        [InverseProperty(nameof(Reply.Ticket))]
        [Ignore]
        [JsonIgnore]
        public ICollection<Reply> Replies { get; } = [];

        /// <summary>
        /// The collection of attachments attached to the ticket.
        /// </summary>
        [Required]
        [InverseProperty(nameof(Attachment.Ticket))]
        [Ignore]
        [JsonIgnore]
        public ICollection<Attachment> Attachments { get; } = [];

        /// <summary>
        /// The collection of notifications related to the ticket.
        /// </summary>
        [Required]
        [InverseProperty(nameof(Notification.Ticket))]
        [Ignore]
        [JsonIgnore]
        public ICollection<Notification> Notifications { get; } = [];
    }
}
