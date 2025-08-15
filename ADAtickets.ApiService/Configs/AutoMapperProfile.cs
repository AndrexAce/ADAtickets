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

using ADAtickets.Shared.Dtos.Requests;
using ADAtickets.Shared.Dtos.Responses;
using ADAtickets.Shared.Models;
using AutoMapper;

namespace ADAtickets.ApiService.Configs;

/// <summary>
///     Defines the AutoMapper profile to map the entities to DTOs and vice versa.
/// </summary>
internal sealed class AutoMapperProfile : Profile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoMapperProfile" /> class.
    /// </summary>
    public AutoMapperProfile()
    {
        _ = CreateMap<Edit, EditResponseDto>(MemberList.Destination);
        _ = CreateMap<EditRequestDto, Edit>(MemberList.Source);

        _ = CreateMap<Reply, ReplyResponseDto>(MemberList.Destination);
        _ = CreateMap<ReplyRequestDto, Reply>(MemberList.Source);

        _ = CreateMap<Notification, NotificationResponseDto>(MemberList.Destination)
            .ForMember(notificationDto => notificationDto.Recipients,
                opt => opt.MapFrom(src =>
                    src.UserNotifications.Where(un => un.NotificationId == src.Id).Select(un => un.ReceiverUserId)));
        _ = CreateMap<NotificationRequestDto, Notification>(MemberList.Source);

        _ = CreateMap<Platform, PlatformResponseDto>(MemberList.Destination)
            .ForMember(platformDto => platformDto.Tickets,
                opt => opt.MapFrom(src => src.Tickets.Select(ticket => ticket.Id)))
            .ForMember(platformDto => platformDto.UsersPreferred,
                opt =>
                    opt.MapFrom(src => src.UserPlatforms.Where(up => up.PlatformId == src.Id).Select(up => up.UserId)));
        _ = CreateMap<PlatformRequestDto, Platform>(MemberList.Source);

        _ = CreateMap<Ticket, TicketResponseDto>(MemberList.Destination)
            .ForMember(ticketDto => ticketDto.Edits, opt => opt.MapFrom(src => src.Edits.Select(edit => edit.Id)))
            .ForMember(ticketDto => ticketDto.Replies, opt => opt.MapFrom(src => src.Replies.Select(reply => reply.Id)))
            .ForMember(ticketDto => ticketDto.Attachments,
                opt => opt.MapFrom(src => src.Attachments.Select(attachment => attachment.Id)))
            .ForMember(ticketDto => ticketDto.Notifications,
                opt => opt.MapFrom(src => src.Notifications.Select(notifications => notifications.Id)))
            .ForMember(ticketDto => ticketDto.CreatorName,
                opt => opt.MapFrom(src => $"{src.CreatorUser.Name} {src.CreatorUser.Surname}"))
            .ForMember(ticketDto => ticketDto.LastUpdateDateTime,
                opt => opt.MapFrom(src =>
                    src.Edits.Any() ? src.Edits.MaxBy(edit => edit.EditDateTime)!.EditDateTime : src.CreationDateTime));
        _ = CreateMap<TicketRequestDto, Ticket>(MemberList.Source)
            .ForSourceMember(ticketDto => ticketDto.Requester, opt => opt.DoNotValidate());

        _ = CreateMap<User, UserResponseDto>(MemberList.Destination)
            .ForMember(userDto => userDto.CreatedTickets,
                opt => opt.MapFrom(src => src.CreatedTickets.Select(ticket => ticket.Id)))
            .ForMember(userDto => userDto.AssignedTickets,
                opt => opt.MapFrom(src => src.AssignedTickets.Select(ticket => ticket.Id)))
            .ForMember(userDto => userDto.Replies, opt => opt.MapFrom(src => src.Replies.Select(reply => reply.Id)))
            .ForMember(userDto => userDto.Edits, opt => opt.MapFrom(src => src.Edits.Select(edit => edit.Id)))
            .ForMember(userDto => userDto.PreferredPlatforms,
                opt =>
                    opt.MapFrom(src => src.UserPlatforms.Where(up => up.UserId == src.Id).Select(up => up.PlatformId)))
            .ForMember(userDto => userDto.SentNotifications,
                opt => opt.MapFrom(src => src.SentNotifications.Select(notification => notification.Id)))
            .ForMember(userDto => userDto.ReceivedNotifications,
                opt => opt.MapFrom(src =>
                    src.UserNotifications.Where(un => un.ReceiverUserId == src.Id).Select(un => un.NotificationId)));
        _ = CreateMap<UserRequestDto, User>(MemberList.Source);

        _ = CreateMap<Attachment, AttachmentResponseDto>(MemberList.Destination)
            .ForMember(attachmentDto => attachmentDto.Path,
                opt => opt.MapFrom(src => Path.GetRelativePath(Directory.GetCurrentDirectory(), src.Path)));
        _ = CreateMap<AttachmentRequestDto, Attachment>(MemberList.Source)
            .ForMember(attachment => attachment.Path, opt => opt.MapFrom(src => src.Name))
            .ForSourceMember(attachmentDto => attachmentDto.Content, opt => opt.DoNotValidate());

        _ = CreateMap<UserPlatform, UserPlatformResponseDto>(MemberList.Destination);
        _ = CreateMap<UserPlatformRequestDto, UserPlatform>(MemberList.Source);

        _ = CreateMap<UserNotification, UserNotificationResponseDto>(MemberList.Destination);
        _ = CreateMap<UserNotificationRequestDto, UserNotification>(MemberList.Source);
    }
}