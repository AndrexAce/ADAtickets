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

using ADAtickets.Shared.Dtos.Forms;
using ADAtickets.Shared.Dtos.Requests;
using ADAtickets.Shared.Dtos.Responses;
using AutoMapper;

namespace ADAtickets.Web.Utilities;

/// <summary>
///     Defines the AutoMapper profile to map the Response DTOs to Request DTOs.
/// </summary>
internal sealed class AutoMapperProfile : Profile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoMapperProfile" /> class.
    /// </summary>
    public AutoMapperProfile()
    {
        _ = CreateMap<EditResponseDto, EditRequestDto>(MemberList.Destination);

        _ = CreateMap<ReplyResponseDto, ReplyRequestDto>(MemberList.Destination);

        _ = CreateMap<NotificationResponseDto, NotificationRequestDto>(MemberList.Destination);

        _ = CreateMap<PlatformResponseDto, PlatformRequestDto>(MemberList.Destination);

        _ = CreateMap<TicketResponseDto, TicketRequestDto>(MemberList.Destination)
            .ForMember(ticketRequest => ticketRequest.Requester, opt => opt.Ignore());

        _ = CreateMap<UserResponseDto, UserRequestDto>(MemberList.Destination);

        _ = CreateMap<AttachmentResponseDto, AttachmentRequestDto>(MemberList.Destination)
            .ForMember(attachmentRequest => attachmentRequest.Name, opt => opt.MapFrom(src => src.Path))
            .ForMember(attachmentRequest => attachmentRequest.Content, opt => opt.Ignore());

        _ = CreateMap<UserPlatformResponseDto, UserPlatformRequestDto>(MemberList.Destination);

        _ = CreateMap<UserNotificationResponseDto, UserNotificationRequestDto>(MemberList.Destination);

        _ = CreateMap<TicketResponseDto, TicketFormDto>(MemberList.Destination)
            .ForMember(ticketForm => ticketForm.Requester, opt => opt.Ignore());
        _ = CreateMap<TicketFormDto, TicketRequestDto>(MemberList.Destination);
    }
}