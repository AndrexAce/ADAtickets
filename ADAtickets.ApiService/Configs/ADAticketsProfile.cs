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
using ADAtickets.ApiService.Dtos;
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
            CreateMap<Edit, EditDto>();
            CreateMap<EditDto, Edit>();
        }
    }
}
