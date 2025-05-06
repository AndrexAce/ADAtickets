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
using ADAtickets.ApiService.Dtos.Requests;
using ADAtickets.ApiService.Dtos.Responses;
using ADAtickets.ApiService.Models;
using AutoMapper;

namespace ADAtickets.ApiService.Configs
{
    /// <summary>
    /// Defines the AutoMapper profile to map the entities to DTOs and vice versa.
    /// </summary>
    sealed class ADAticketsProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ADAticketsProfile"/> class.
        /// </summary>
        public ADAticketsProfile()
        {
            CreateMap<Edit, EditResponseDto>();
            CreateMap<EditRequestDto, Edit>();

            CreateMap<Reply, ReplyResponseDto>();
            CreateMap<ReplyRequestDto, Edit>();

            CreateMap<Notification, NotificationResponseDto>()
                .ForMember(notificationDto => notificationDto.Recipients, opt => opt.MapFrom(src => src.Recipients.Select(recipient => recipient.Id)));
            CreateMap<NotificationRequestDto, Notification>();

            CreateMap<Platform, PlatformResponseDto>()
                .ForMember(platformDto => platformDto.Tickets, opt => opt.MapFrom(src => src.Tickets.Select(ticket => ticket.Id)))
                .ForMember(platformDto => platformDto.UsersPreferred, opt => opt.MapFrom(src => src.UsersPreferred.Select(user => user.Id)));
            CreateMap<PlatformRequestDto, Platform>();

            CreateMap<Ticket, TicketResponseDto>()
                .ForMember(ticketDto => ticketDto.Edits, opt => opt.MapFrom(src => src.Edits.Select(edit => edit.Id)))
                .ForMember(ticketDto => ticketDto.Replies, opt => opt.MapFrom(src => src.Replies.Select(reply => reply.Id)))
                .ForMember(ticketDto => ticketDto.Attachments, opt => opt.MapFrom(src => src.Attachments.Select(attachment => attachment.Id)))
                .ForMember(ticketDto => ticketDto.Notifications, opt => opt.MapFrom(src => src.Notifications.Select(notifications => notifications.Id)));
            CreateMap<TicketRequestDto, Ticket>();

            CreateMap<User, UserResponseDto>()
                .ForMember(userDto => userDto.CreatedTickets, opt => opt.MapFrom(src => src.CreatedTickets.Select(ticket => ticket.Id)))
                .ForMember(userDto => userDto.AssignedTickets, opt => opt.MapFrom(src => src.AssignedTickets.Select(ticket => ticket.Id)))
                .ForMember(userDto => userDto.Replies, opt => opt.MapFrom(src => src.Replies.Select(reply => reply.Id)))
                .ForMember(userDto => userDto.Edits, opt => opt.MapFrom(src => src.Edits.Select(edit => edit.Id)))
                .ForMember(userDto => userDto.PreferredPlatforms, opt => opt.MapFrom(src => src.PreferredPlatforms.Select(platform => platform.Id)))
                .ForMember(userDto => userDto.SentNotifications, opt => opt.MapFrom(src => src.SentNotifications.Select(notification => notification.Id)))
                .ForMember(userDto => userDto.ReceivedNotifications, opt => opt.MapFrom(src => src.ReceivedNotifications.Select(notification => notification.Id)));
            CreateMap<UserRequestDto, User>();

            CreateMap<Attachment, AttachmentResponseDto>();
            CreateMap<AttachmentRequestDto, Attachment>()
                .ForMember(attachment => attachment.Path, opt => opt.MapFrom(src => src.Name));
        }
    }
}
