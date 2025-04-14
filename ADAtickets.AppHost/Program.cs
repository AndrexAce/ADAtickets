var builder = DistributedApplication.CreateBuilder(args);

var dbUsername = builder.AddParameter(name: "postgresUsername", secret: true);
var dbPassword = builder.AddParameter(name: "postgresPassword", secret: true);

var postgresdb = builder.AddPostgres(name: "PostgreSQL", userName: dbUsername, password: dbPassword, port: 5432)
    .WithPgWeb()
    .WithDataVolume(name: "postgres_data", isReadOnly: false)
    .AddDatabase(name: "ADAticketsDatabase", "ADAticketsDatabase");

var apiService = builder.AddProject<Projects.ADAtickets_ApiService>(name: "ADAticketsApi")
    .WithReference(source: postgresdb)
    .WaitFor(dependency: postgresdb);

builder.AddProject<Projects.ADAtickets_Web>(name: "ADAticketsWeb")
    .WithExternalHttpEndpoints()
    .WithReference(source: apiService)
    .WaitFor(dependency: apiService);

builder.Build().Run();