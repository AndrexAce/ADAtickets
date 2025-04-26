using ADAtickets.ApiService.Configs;
using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;
using ADAtickets.ApiService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(args);

// Add the Azure authentication services.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Add services commonly used with controllers APIs.
builder.Services
    .AddControllers(options => // Require the APIs to respect the broswer request media type, and return a 406 Not Acceptable response if the media type is not supported.
    {
        options.RespectBrowserAcceptHeader = true;
        options.ReturnHttpNotAcceptable = true;
    })
    .ConfigureApiBehaviorOptions(options => // Configure the API behavior options to return either a JSON or XML 400 Bad Request response when the model state is invalid.
    {
        options.InvalidModelStateResponseFactory = context =>
            new BadRequestObjectResult(context.ModelState)
            {
                ContentTypes = { MediaTypeNames.Application.Json, MediaTypeNames.Application.Xml }
            };
    })
    .AddXmlSerializerFormatters(); // Add XML serialization support for the APIs.

// Add OpenAPI support for the APIs.
builder.Services.AddOpenApi();

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services used to return detailed error messages for failed requests.
builder.Services.AddProblemDetails();

// Add the DBContext to execute queries against the database.
// The context is pooled so that the application spends less time creating and destroying contexts.
builder.Services.AddDbContextPool<ADAticketsDbContext>(options =>
{
    // Configure the DBContext to use PostgreSQL.
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL"), options =>
    {
        // Create the enumerations in the connected database.
        options.MapEnum<Priority>("priority")
            .MapEnum<Status>("status")
            .MapEnum<TicketType>("ticket_type")
            .MapEnum<UserType>("user_type")
            .EnableRetryOnFailure();
    })
        .UseSnakeCaseNamingConvention();
});

// Configure the scoped (one per request) classes available for dependency injection.
builder.Services.AddScoped<IEditRepository, EditRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IPlatformRepository, PlatformRepository>();
builder.Services.AddScoped<IReplyRepository, ReplyRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add automapping of entities.
builder.Services.AddAutoMapper(typeof(ADAticketsProfile));

var app = builder.Build();

// Apply migrations on startup if the app is in development to ensure the database is up to date.
if (app.Environment.IsDevelopment())
{
    var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ADAticketsDbContext>();
    await db.Database.MigrateAsync();
}

// Create an endpoint to access the Swagger UI if the app is in development.
// Create an exception handler to show a personalised error message based on the environment.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseExceptionHandler("/error-dev");
}
else
{
    app.UseExceptionHandler("/error");
}

// Configure an interceptor for 4xx and 5xx errors to return a JSON response with the error details.
app.UseStatusCodePages();

// Add the redirection from HTTP to HTTPS.
app.UseHttpsRedirection();

// Add the authentication service for the API.
app.UseAuthorization();

// Add the controllers endpoints.
app.MapControllers();

// Start the application.
await app.RunAsync();
