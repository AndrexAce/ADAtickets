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
using ADAtickets.Shared.Models;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using System.Text.RegularExpressions;
using PlatformService = ADAtickets.ApiService.Services.PlatformRepository;

namespace ADAtickets.ApiService.Tests.Services.PlatformRepository
{
    /// <summary>
    /// <c>AddPlatformAsync(Platform)</c>
    /// <list type="number">
    ///     <item>Valid entity</item>
    ///     <item>Invalid entity</item>
    /// </list>
    /// </summary>
    public class PostTests
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
        public async Task AddPlatform_ValidEntity_ReturnsPlatform(Platform inPlatform)
        {
            // Arrange
            List<Platform> platforms = [];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<DbSet<Platform>> mockPlatformSet = platforms.BuildMockDbSet();
            _ = mockPlatformSet.Setup(s => s.Add(It.IsAny<Platform>()))
                .Callback<Platform>(p =>
                {
                    if (p.Name.Length <= 254 && Regex.IsMatch(p.RepositoryUrl, @"^(https?:\/\/)?(www\.)?([a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,}\/?$"))
                    {
                        platforms.Add(p);
                    }
                });
            _ = mockContext.Setup(c => c.Platforms)
                .Returns(mockPlatformSet.Object);

            PlatformService service = new(mockContext.Object);

            CancellationToken cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.AddPlatformAsync(inPlatform);
            Platform? addedPlatform = await mockContext.Object.Platforms.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.NotNull(addedPlatform);
            Assert.NotEmpty(platforms);
        }

        [Theory]
        [MemberData(nameof(InvalidPlatformData))]
        public async Task AddPlatform_InvalidEntity_ReturnsNothing(Platform inPlatform)
        {
            // Arrange
            List<Platform> platforms = [];

            Mock<ADAticketsDbContext> mockContext = new();
            Mock<DbSet<Platform>> mockPlatformSet = platforms.BuildMockDbSet();
            _ = mockPlatformSet.Setup(s => s.Add(It.IsAny<Platform>()))
                .Callback<Platform>(p =>
                {
                    if (p.Name.Length <= 254 && Regex.IsMatch(p.RepositoryUrl, @"^(https?:\/\/)?(www\.)?([a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,}\/?$"))
                    {
                        platforms.Add(p);
                    }
                });
            _ = mockContext.Setup(c => c.Platforms)
                .Returns(mockPlatformSet.Object);

            PlatformService service = new(mockContext.Object);

            CancellationToken cancellationToken = TestContext.Current.CancellationToken;

            // Act
            await service.AddPlatformAsync(inPlatform);
            Platform? addedPlatform = await mockContext.Object.Platforms.SingleOrDefaultAsync(cancellationToken);

            // Assert
            Assert.Null(addedPlatform);
            Assert.Empty(platforms);
        }
    }
}
