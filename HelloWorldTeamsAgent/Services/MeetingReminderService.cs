using HelloWorldTeamsAgent.Tools;
using Microsoft.Agents.Core.Models;
using Microsoft.Agents.Hosting.AspNetCore;

namespace HelloWorldTeamsAgent.Services;

public sealed class MeetingReminderService(
    ConversationReferenceStore store,
    GraphMeetingService graphMeetingService,
    IConfiguration configuration,
    CloudAdapter cloudAdapter) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var reference in store.GetActiveConversations())
            {
                if (reference is null)
                {
                    continue;
                }

                await SendReminderAsync(reference, stoppingToken);
                store.Remove(reference.Conversation.Id);
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private async Task SendReminderAsync(ConversationReference reference, CancellationToken cancellationToken)
    {
        var aadObjectId = MeetingPreparationTool.GetUserIdFromGraphClient();

        var meetingInfo = await graphMeetingService.GetMeetingBriefingAsync(aadObjectId, cancellationToken);

        if (string.IsNullOrWhiteSpace(meetingInfo))
        {
            return;
        }

        var activity = Activity.CreateMessageActivity()
            .WithText($"Reminder: You have an upcoming meeting. Here are the details:\n{meetingInfo}");

        var agentAppId = configuration["AzureAd:ClientId"];

        await cloudAdapter.ContinueConversationAsync(
            agentAppId,
            reference,
            async (turnContext, ct) =>
            {
                await turnContext.SendActivityAsync(activity, ct);
            },
            cancellationToken);
    }
}
