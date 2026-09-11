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

//app.MapDefaultAgentEndpoints();
app.MapAgentEndpoints(requireAuth: false);

app.Run();
