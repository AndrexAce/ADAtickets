using YamlDotNet.Core.Tokens;

var builder = DistributedApplication.CreateBuilder(args);

var dbUsername = builder.AddParameter(name: "postgresUsername", secret: true);
var dbPassword = builder.AddParameter(name: "postgresPassword", secret: true);

var postgresdb = builder.AddPostgres(name: "adatickets-postgresql", userName: dbUsername, password: dbPassword, port: 5432)
    .WithPgWeb(options =>
    {
        options.WithContainerName(name: "adatickets-pgweb");
        options.WithUrlForEndpoint(endpointName: "http", callback: u => u.DisplayText = "Pgweb HTTP");
    }, containerName: "adatickets-pgweb")
    .WithContainerName(name: "adatickets-postgresql")
    .WithDataVolume(name: "adatickets-database-data", isReadOnly: false)
    .AddDatabase(name: "adatickets-database", databaseName: "adatickets-database");

var apiService = builder.AddDockerfile(name: "adatickets-api", contextPath: "..", dockerfilePath: "ADAtickets.ApiService/Dockerfile")
    .WithContainerName(name: "adatickets-api")
    .WithImageTag("dev")
    .WithHttpEndpoint(targetPort: 8080, port: 5194)
    .WithHttpsEndpoint(targetPort: 8081, port: 7214)
    .WithUrlForEndpoint(endpointName: "http", callback: u => u.DisplayText = "API HTTP")
    .WithUrlForEndpoint(endpointName: "https", callback: u => u.DisplayText = "API HTTPS")
    .WithReference(source: postgresdb)
    .WaitFor(dependency: postgresdb);

builder.AddDockerfile(name: "adatickets-web", contextPath: "..", dockerfilePath: "ADAtickets.Web/Dockerfile")
    .WithContainerName(name: "adatickets-web")
    .WithImageTag("dev")
    .WithExternalHttpEndpoints()
    .WithHttpEndpoint(targetPort: 8080, port: 5207)
    .WithHttpsEndpoint(targetPort: 8081, port: 7013)
    .WithUrlForEndpoint(endpointName: "http", callback: u => u.DisplayText = "Web HTTP")
    .WithUrlForEndpoint(endpointName: "https", callback: u => u.DisplayText = "Web HTTPS")
    .WaitFor(dependency: apiService);

builder.Build().Run();