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

var apiService = builder.AddProject<Projects.ADAtickets_ApiService>(name: "adatickets-api")
    .WithUrlForEndpoint(endpointName: "http", callback: u => u.DisplayText = "API HTTP")
    .WithUrlForEndpoint(endpointName: "https", callback: u => u.DisplayText = "API HTTPS")
    .WithReference(source: postgresdb)
    .WaitFor(dependency: postgresdb);

builder.AddProject<Projects.ADAtickets_Web>(name: "adatickets-web")
    .WithExternalHttpEndpoints()
    .WithUrlForEndpoint(endpointName: "http", callback: u => u.DisplayText = "Web HTTP")
    .WithUrlForEndpoint(endpointName: "https", callback: u => u.DisplayText = "Web HTTPS")
    .WithReference(source: apiService)
    .WaitFor(dependency: apiService);

builder.Build().Run();