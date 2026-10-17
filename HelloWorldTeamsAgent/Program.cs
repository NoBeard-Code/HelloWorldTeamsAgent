using Azure.AI.OpenAI;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Storage;
using System.ClientModel;
using Microsoft.Extensions.AI;
using HelloWorldTeamsAgent.Services;
using HelloWorldTeamsAgent.Tools;
using HelloWorldTeamsAgent.Agents;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<GraphMeetingService>();
builder.Services.AddSingleton<MeetingPreparationTool>();

builder.Services.AddSingleton<ConversationReferenceStore>();
builder.Services.AddHostedService<MeetingReminderService>();

builder.Services.AddAuthorization();

// this forces User Secrets to load even in non-Development
builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);

var endpoint = builder.Configuration["AzureOpenAI:Endpoint"]!;
var apiKey = builder.Configuration["AzureOpenAI:ApiKey"]!;
var model = builder.Configuration["AzureOpenAI:DeploymentName"]!;

var openAIClient = new AzureOpenAIClient(
    new Uri(endpoint), 
    new ApiKeyCredential(apiKey),
    new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2024_10_21));

var chatClient = openAIClient
    .GetChatClient(model)
    .AsIChatClient();

builder.Services.AddChatClient(chatClient)
    .UseFunctionInvocation()
    .UseLogging();

builder.AddAgentApplicationOptions();
builder.AddAgent<MeetingPreparationAgent>();

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
