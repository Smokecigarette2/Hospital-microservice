using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var ocelotFile = builder.Environment.IsEnvironment("Docker")
    ? "ocelot.docker.json"
    : "ocelot.json";

builder.Configuration.AddJsonFile(ocelotFile, optional: false, reloadOnChange: true);

builder.Services.AddOcelot();

var app = builder.Build();

app.UseHealthChecks("/health");

await app.UseOcelot();

app.Run();
