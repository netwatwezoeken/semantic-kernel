using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime;
using Microsoft.SemanticKernel.Agents.Runtime.Core;

namespace _06_AgentFramework;

internal sealed class CustomSequentialActor :
    AgentActor,
    IHandle<CustomSequentialMessages.Request>,
    IHandle<CustomSequentialMessages.Response>
{
    private readonly AgentType _nextAgent;

    /// <summary>
    /// Initializes a new instance of the <see cref="SequentialActor"/> class.
    /// </summary>
    /// <param name="id">The unique identifier of the agent.</param>
    /// <param name="runtime">The runtime associated with the agent.</param>
    /// <param name="context">The orchestration context.</param>
    /// <param name="agent">An <see cref="Agent"/>.</param>
    /// <param name="nextAgent">The identifier of the next agent for which to handoff the result</param>
    /// <param name="logger">The logger to use for the actor</param>
    public CustomSequentialActor(AgentId id, IAgentRuntime runtime, OrchestrationContext context, Agent agent, AgentType nextAgent, ILogger<CustomSequentialActor>? logger = null)
        : base(id, runtime, context, agent, logger)
    {
        logger?.LogInformation("ACTOR {ActorId} {NextAgent}", this.Id, nextAgent);
        this._nextAgent = nextAgent;
    }

    /// <inheritdoc/>
    public async ValueTask HandleAsync(CustomSequentialMessages.Request item, MessageContext messageContext)
    {
        await this.InvokeAgentAsync(item.Messages, messageContext).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask HandleAsync(CustomSequentialMessages.Response item, MessageContext messageContext)
    {
        await this.InvokeAgentAsync([item.Message], messageContext).ConfigureAwait(false);
    }

    private async ValueTask InvokeAgentAsync(IList<ChatMessageContent> input, MessageContext messageContext)
    {
        this.Logger.LogInformation("INVOKE {ActorId} {NextAgent}", this.Id, this._nextAgent);
        foreach (ChatMessageContent message in input)
        {
            message.Content = Regex.Replace(message.Content ?? "", @"<think>.*?</think>", "", RegexOptions.Singleline);
            message.Content = message.Content.Replace(@"<think>", "");
            message.Content = message.Content.Replace(@"</think>", "");
        }

        ChatMessageContent response = await this.InvokeAsync(input, messageContext.CancellationToken).ConfigureAwait(false);

        await this.SendMessageAsync(response.AsResponseMessage(), this._nextAgent, messageContext.CancellationToken).ConfigureAwait(false);
    }
}