using Microsoft.Agents.Core.Models;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Extensions.AI;

namespace HelloWorldTeamsAgent.Services;

public sealed class MeetingReminderService(
    ConversationReferenceStore store,
    GraphMeetingService graphMeetingService,
    IConfiguration configuration,
    IChatClient chatClient,
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
        var aadObjectId = reference.User?.AadObjectId;

        var meetingInfo = await graphMeetingService.GetMeetingBriefingAsync(aadObjectId, cancellationToken);

        if (string.IsNullOrWhiteSpace(meetingInfo))
        {
            return;
        }

        var reminderText = await BuildReminderMessageAsync(
                    meetingInfo,
                    cancellationToken);

        var activity = Activity.CreateMessageActivity()
            .WithText(reminderText);

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

    private async Task<string> BuildReminderMessageAsync(
        string briefing,
        CancellationToken cancellationToken)
    {
        var response = await chatClient.GetResponseAsync(
        [
            new ChatMessage(
                ChatRole.System,
                "You are a Microsoft Teams meeting assistant. Write a brief proactive reminder in plain language, friendly and concise. Mention the meeting is starting soon and ask if the user wants a briefing."),
            new ChatMessage(ChatRole.User, briefing)
        ],
        cancellationToken: cancellationToken);

        return response.Text;
    }
}
