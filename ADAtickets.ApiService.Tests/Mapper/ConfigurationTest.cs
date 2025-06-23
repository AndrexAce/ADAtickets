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
using ADAtickets.ApiService.Configs;
using AutoMapper;

namespace ADAtickets.ApiService.Tests.Mapper
{
    sealed public class ConfigurationTest
    {
        [Fact]
        public void ValidConfiguration_DoesNotThrow()
        {
            // Arrange
            var mapper = new MapperConfiguration(cfg => { cfg.AddProfile<ADAticketsProfile>(); }).CreateMapper();

            // Act
            var exception = Record.Exception(() => mapper.ConfigurationProvider.AssertConfigurationIsValid());

            // Assert
            Assert.Null(exception);
        }
    }
}
