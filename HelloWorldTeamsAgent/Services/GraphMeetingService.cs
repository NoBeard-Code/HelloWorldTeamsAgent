using Azure.Identity;
using Microsoft.Graph;
using System.Text.Json;

namespace HelloWorldTeamsAgent.Services;

public sealed class GraphMeetingService
{
    private readonly GraphServiceClient? _graphClient;

    public bool IsConfigured => _graphClient is not null;

    public GraphMeetingService(IConfiguration configuration)
    {
        var credential = new ClientSecretCredential(
            tenantId: configuration["AzureAd:TenantId"],
            clientId: configuration["AzureAd:ClientId"],
            clientSecret: configuration["AzureAd:ClientSecret"]);

        string[] scopes = configuration.GetSection("AzureAd:Scopes").Get<string[]>();

        _graphClient = new GraphServiceClient(credential, scopes);
    }

    public async Task<string?> GetMeetingBriefingAsync(string? aadObjectId, CancellationToken cancellationToken = default)
    {
        if (_graphClient is null || string.IsNullOrWhiteSpace(aadObjectId))
        {
            return null;
        }

        try
        {
            var upcomingEvents = await _graphClient
                .Users[aadObjectId]
                .Calendar
                .Events
                .GetAsync(q => q.QueryParameters.Top = 3,
                cancellationToken);

            if (upcomingEvents?.Value is not { Count: > 0 } events)
            {
                return null;
            }

            var nextMeeting = events[0];

            return JsonSerializer.Serialize(nextMeeting);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error retrieving meeting briefing: {ex.Message}");
            return null;
        }
    }   
}
