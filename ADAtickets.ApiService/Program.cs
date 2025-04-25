using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;
using ADAtickets.ApiService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add the Azure authentication services.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// Add services commonly used with controllers APIs.
builder.Services.AddControllers();

// Add OpenAPI support for the APIs.
builder.Services.AddOpenApi("APIs");

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

var app = builder.Build();

// Apply migrations on startup if the app is in development to ensure the database is up to date.
if (app.Environment.IsDevelopment())
{
    var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ADAticketsDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Create an endpoint to access the Swagger UI if the app is in development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add the redirection from HTTP to HTTPS.
app.UseHttpsRedirection();

// Add the authentication service for the API.
app.UseAuthorization();

// Add the controllers endpoints.
app.MapControllers();

// Start the application.
await app.RunAsync();
