var builder = DistributedApplication.CreateBuilder(args);

var dbUsername = builder.AddParameter(name: "postgresUsername", secret: true);
var dbPassword = builder.AddParameter(name: "postgresPassword", secret: true);

var postgresdb = builder.AddPostgres(name: "postgres", userName: dbUsername, password: dbPassword, port: 5432)
    .WithDataVolume(name: "postgres_data", isReadOnly: false)
    .AddDatabase(name: "ADAtickets", "ADAtickets");

var apiService = builder.AddProject<Projects.ADAtickets_ApiService>(name: "apiservice")
    .WithReference(source: postgresdb)
    .WithHttpEndpoint(name: "apiService")
    .WaitFor(dependency: postgresdb);

builder.AddProject<Projects.ADAtickets_Web>(name: "webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(source: apiService)
    .WaitFor(dependency: apiService);

builder.Build().Run();