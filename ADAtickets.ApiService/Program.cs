using ADAtickets.ApiService.Configs;
using ADAtickets.ApiService.Models;
using ADAtickets.ApiService.Repositories;
using ADAtickets.ApiService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json.Serialization;

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
    .AddXmlSerializerFormatters() // Add XML serialization support for the APIs.
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())); // Configure enumerations serialization support.

// Add Swagger documentation for the APIs.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "ADAtickets API",
        Description = "Web MCV APIs to interact with the ADAtickets ticketing system.",
        Contact = new OpenApiContact
        {
            Name = "Andrea Lucchese",
            Email = "andrylook14@gmail.com"
        },
        License = new OpenApiLicense
        {
            Name = "GPL v3",
            Url = new("https://github.com/AndrexAce/ADAtickets/blob/master/LICENSE.txt")
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

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

if (app.Environment.IsDevelopment())
{
    // Apply migrations on startup if the app is in development to ensure the database is up to date.
    var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ADAticketsDbContext>();
    await db.Database.MigrateAsync();

    // Create an endpoint to access the API documentation.
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "/openapi/{documentName}.json";
    });

    // Create an endpoint to access the API documentation via Scalar.
    app.MapScalarApiReference(options =>
    {
        options.Theme = ScalarTheme.BluePlanet;
    });

    // When exceptions happen dunring development, show a detailed screen.
    app.UseDeveloperExceptionPage();
}

// Create an exception handler to handle exceptions in APIs.
app.UseExceptionHandler();

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
