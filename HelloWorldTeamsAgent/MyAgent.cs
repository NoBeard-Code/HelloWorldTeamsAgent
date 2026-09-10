using Azure.Identity;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.Agents.Storage;
using Microsoft.Graph;

namespace HelloWorldTeamsAgent;

public class MyAgent : AgentApplication
{
    public MyAgent(IStorage storage) : base(new AgentApplicationOptions(storage)
    {
        TurnStateFactory = () => new TurnState(storage)
    })
    { }

    [MessageRoute]
    public async Task OnMessageAsync(ITurnContext turnContext, ITurnState turnState, CancellationToken cancellationToken)
    {
        var text = turnContext.Activity.Text ?? "(no text)";

        //var graph = new GraphServiceClient(new DefaultAzureCredential());
        //var me = await graph.Me.GetAsync();
        //var userName = me?.DisplayName ?? "unknown user";
        var userName = turnContext.Activity.From?.Name ?? "unknown user";

        var activity = Activity.CreateMessageActivity()
            .WithText($"Hello {userName} from Agent! You said: {text}");

        await turnContext.SendActivityAsync(activity, cancellationToken);
    }
}
