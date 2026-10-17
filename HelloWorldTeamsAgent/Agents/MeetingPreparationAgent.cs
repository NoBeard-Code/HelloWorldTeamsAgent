using HelloWorldTeamsAgent.Services;
using HelloWorldTeamsAgent.Tools;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.Agents.Storage;
using Microsoft.Extensions.AI;

namespace HelloWorldTeamsAgent.Agents;

public class MeetingPreparationAgent(
    IStorage storage,
    ConversationReferenceStore conversationReferenceStore,
    IChatClient chatClient,
    MeetingPreparationTool meetingPreparationTool) 
    : AgentApplication(new AgentApplicationOptions(storage)
    {
        TurnStateFactory = () => new TurnState(storage)
    })
{
    [MessageRoute]
    public async Task OnMessageAsync(
        ITurnContext turnContext,
        ITurnState turnState,
        CancellationToken cancellationToken)
    {
        var reference = turnContext.Activity.GetConversationReference();
        conversationReferenceStore.Save(reference.Conversation.Id, reference);

        var userInput = turnContext.Activity.Text ?? "(no text)";

        var aadObjectId = turnContext.Activity.From?.AadObjectId;

        var tools = new[]
        {
            AIFunctionFactory.Create(meetingPreparationTool.GetMeetingBriefingAsync)
        };

        //var graph = new GraphServiceClient(new DefaultAzureCredential());
        //var me = await graph.Me.GetAsync();
        //var userName = me?.DisplayName ?? "unknown user";

        // Let the model choose whether to call the meeting tool.
        // The tool is the real capability; this is just the agent orchestration.
        var response = await chatClient.GetResponseAsync(
        [
            new ChatMessage(
                ChatRole.System, """
                You are a helpful assistant that can provide a briefing for the user's next meeting,
                including time, attendees, recent discussion, and likely open actions. 
                You have access to a tool called 'GetMeetingBriefing' that can retrieve this information. 
                If the user asks about their next meeting or requests a briefing, you should call the 'GetMeetingBriefing' tool. 
                Otherwise, respond to the user's input as a helpful assistant.
                """),
            new ChatMessage(ChatRole.User, userInput)
        ],
        new ChatOptions
        {
            Tools = tools
        },
        cancellationToken: cancellationToken);

        var activity = Activity.CreateMessageActivity()
            .WithText(response.Text);

        await turnContext.SendActivityAsync(activity, cancellationToken);
    }
}
