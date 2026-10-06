using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.Agents.Storage;
using Microsoft.Extensions.AI;

namespace HelloWorldTeamsAgent;

public class MyAgent(IStorage storage, IChatClient chatClient) 
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
        var userInput = turnContext.Activity.Text ?? "(no text)";

        var response = await chatClient.GetResponseAsync(
        [
            new ChatMessage(
                ChatRole.System,
                "You are a helpful assistant for Microsoft Teams. Keep responses concise and friendly."),
            new ChatMessage(ChatRole.User, userInput)
        ],
        cancellationToken: cancellationToken);

        var replyText = response.Text;

        var activity = Activity.CreateMessageActivity()
            .WithText(replyText);

        await turnContext.SendActivityAsync(activity, cancellationToken);
    }
}
