using HelloWorldTeamsAgent;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddHttpClient();

builder.Services.AddAuthorization();

builder.AddAgentApplicationOptions();

builder.AddAgent<MyAgent>();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.MapDefaultAgentEndpoints();
}
else
{
    // use Playground in development for testing and debugging:
    // agentsplayground -e "https://c58c37m6-5163.eun1.devtunnels.ms/api/messages" -c "emulator"

    app.MapAgentEndpoints(requireAuth: false);
}

app.Run();
