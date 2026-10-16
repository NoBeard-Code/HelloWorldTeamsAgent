using Azure.Identity;
using HelloWorldTeamsAgent.Services;
using Microsoft.Graph;
using System.ComponentModel;

namespace HelloWorldTeamsAgent.Tools;

public sealed class MeetingPreparationTool(GraphMeetingService graphMeetingService)
{
    [Description("Get the next meeting briefing, including time, attendees, recent discussion, and likely open actions.")]
    public async Task<string> GetMeetingBriefingAsync(string userRequest, CancellationToken cancellationToken = default)
    {       
        var aadObjectId = GetUserIdFromGraphClient();

        return await graphMeetingService.GetMeetingBriefingAsync(aadObjectId, cancellationToken);
    }

    private static string GetUserIdFromGraphClient()
    {
        var credential = new DefaultAzureCredential();
        var graphClient = new GraphServiceClient(credential);
        var me = graphClient.Me.GetAsync().Result;
        return me?.Id ?? string.Empty;
    }
}
