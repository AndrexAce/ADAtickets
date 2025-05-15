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
using ADAtickets.ApiService.Configs;
using ADAtickets.ApiService.Models;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using System.Text.RegularExpressions;
using PlatformService = ADAtickets.ApiService.Services.PlatformRepository;

namespace ADAtickets.ApiService.Tests.Services.PlatformRepository
{
    /// <summary>
    /// <c>UpdatePlatform(Platform)</c>
    /// <list type="number">
    ///     <item>Valid entity</item>
    ///     <item>Invalid entity</item>
    /// </list>
    /// </summary>
    public class PutTests
    {
        public static TheoryData<Platform> InvalidPlatformData =>
        [
            Utilities.CreatePlatform(name: new string('a', 255), repositoryUrl: "https://example.com"),
            Utilities.CreatePlatform(name: "Name", repositoryUrl: "://example.com"),
        ];

        public static TheoryData<Platform> ValidPlatformData =>
        [
            Utilities.CreatePlatform(name: "Name", repositoryUrl : "https://example.com")
        ];

        [Theory]
        [MemberData(nameof(ValidPlatformData))]
        public async Task UpdatePlatform_ValidEntity_ReturnsNew(Platform inPlatform)
        {
            // Arrange
            var platforms = new List<Platform> { new() { Id = inPlatform.Id, RepositoryUrl = "http://example.com" } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockPlatformSet = platforms.BuildMockDbSet();
            mockPlatformSet.Setup(s => s.Update(It.IsAny<Platform>()))
                .Callback<Platform>(a =>
                {
                    if (a.Name.Length <= 254 && Regex.IsMatch(a.RepositoryUrl, @"^(https?:\/\/)?(www\.)?([a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,}\/?$"))
                    {
                        platforms[0].RepositoryUrl = inPlatform.RepositoryUrl;
                    }
                });
            mockContext.Setup(c => c.Platforms)
                .Returns(mockPlatformSet.Object);

            var service = new PlatformService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.UpdatePlatformAsync(inPlatform);
            var updatedPlatform = await mockContext.Object.Platforms.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(updatedPlatform);
            Assert.Equal(inPlatform.RepositoryUrl, updatedPlatform.RepositoryUrl);
        }

        [Theory]
        [MemberData(nameof(InvalidPlatformData))]
        public async Task UpdatePlatform_InvalidEntity_ReturnsOld(Platform inPlatform)
        {
            // Arrange
            var platforms = new List<Platform> { new() { Id = inPlatform.Id, RepositoryUrl = "http://example.com" } };

            var mockContext = new Mock<ADAticketsDbContext>();
            var mockPlatformSet = platforms.BuildMockDbSet();
            mockPlatformSet.Setup(s => s.Update(It.IsAny<Platform>()))
                .Callback<Platform>(a =>
                {
                    if (a.Name.Length <= 254 && Regex.IsMatch(a.RepositoryUrl, @"^(https?:\/\/)?(www\.)?([a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,}\/?$"))
                    {
                        platforms[0].RepositoryUrl = inPlatform.RepositoryUrl;
                    }
                });
            mockContext.Setup(c => c.Platforms)
                .Returns(mockPlatformSet.Object);

            var service = new PlatformService(mockContext.Object);

            var cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.UpdatePlatformAsync(inPlatform);
            var updatedPlatform = await mockContext.Object.Platforms.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(updatedPlatform);
            Assert.NotEqual(inPlatform.RepositoryUrl, updatedPlatform.RepositoryUrl);
        }
    }
}
