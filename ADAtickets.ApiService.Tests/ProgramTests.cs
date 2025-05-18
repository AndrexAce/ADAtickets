using ADAtickets.ApiService.Configs;
using ADAtickets.ApiService.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace ADAtickets.ApiService.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void ConfigureServices_RegistersRequiredServices()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            Program.ConfigureServices(builder);

            // Act
            var services = builder.Services.BuildServiceProvider();

            // Assert: Database context and repositories
            Assert.NotNull(services.GetService<ADAticketsDbContext>());
            Assert.NotNull(services.GetService<IEditRepository>());
            Assert.NotNull(services.GetService<INotificationRepository>());
            Assert.NotNull(services.GetService<IPlatformRepository>());
            Assert.NotNull(services.GetService<IReplyRepository>());
            Assert.NotNull(services.GetService<ITicketRepository>());
            Assert.NotNull(services.GetService<IUserRepository>());

            // Assert: AutoMapper
            Assert.NotNull(services.GetService<IMapper>());

            // Assert: Antiforgery
            Assert.NotNull(services.GetService<IAntiforgery>());

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
        }

        [Fact]
        public async Task ConfigureApplication_RegistersMiddlewares()
        {
            // Arrange
            var builder = WebApplication.CreateBuilder();
            Program.ConfigureServices(builder);

            // Act
            var app = builder.Build();

            // Assert
            Assert.Null(await Record.ExceptionAsync(() => Program.ConfigureApplication(app)));
        }
    }
}