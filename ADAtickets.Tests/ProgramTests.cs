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
using ADAtickets.ApiService;
using ADAtickets.ApiService.Configs;
using ADAtickets.ApiService.Controllers;
using ADAtickets.ApiService.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace ADAtickets.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void ConfigureServices_RegistersRequiredServices()
        {
            // Arrange
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            Program.ConfigureServices(builder);

            // Act
            ServiceProvider services = builder.Services.BuildServiceProvider();

            // Assert: Dependency injections
            Assert.NotNull(services.GetService<ADAticketsDbContext>());
            Assert.NotNull(services.GetService<IAttachmentRepository>());
            Assert.NotNull(services.GetService<IEditRepository>());
            Assert.NotNull(services.GetService<INotificationRepository>());
            Assert.NotNull(services.GetService<IPlatformRepository>());
            Assert.NotNull(services.GetService<IReplyRepository>());
            Assert.NotNull(services.GetService<ITicketRepository>());
            Assert.NotNull(services.GetService<IUserRepository>());
            Assert.NotNull(services.GetService<IUserPlatformRepository>());
            Assert.NotNull(services.GetService<IUserNotificationRepository>());
            Assert.NotNull(services.GetService<NotificationsController>());
            Assert.NotNull(services.GetService<EditsController>());
            Assert.NotNull(services.GetService<AzureDevOpsController>());

            // Assert: AutoMapper
            Assert.NotNull(services.GetService<IMapper>());

            // Assert: Authorization
            Assert.NotNull(services.GetService<IAuthorizationService>());

            // Assert: Controllers
            Assert.NotNull(services.GetService<IControllerFactory>());
            Assert.NotNull(services.GetService<IControllerActivator>());

            // Assert: ProblemDetails
            Assert.NotNull(services.GetService<ProblemDetailsFactory>());

            // Assert: Swagger/OpenAPI
            Assert.NotNull(services.GetService<ISwaggerProvider>());

            // Assert: API Explorer
            Assert.NotNull(services.GetService<IApiDescriptionGroupCollectionProvider>());

            // Assert: Cookie authentication
            Assert.NotNull(services.GetService<IAuthenticationSchemeProvider>());

            // Assert: Identity authentication API
            Assert.NotNull(services.GetService<IAuthenticationService>());

            // Assert: Redis cache
            Assert.NotNull(services.GetService<IDistributedCache>());
        }

        [Fact]
        public async Task ConfigureApplication_RegistersMiddlewares()
        {
            // Arrange
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            Program.ConfigureServices(builder);

            // Act
            WebApplication app = builder.Build();

            // Assert
            Assert.Null(await Record.ExceptionAsync(() => Program.ConfigureApplicationAsync(app)));
        }
    }
}