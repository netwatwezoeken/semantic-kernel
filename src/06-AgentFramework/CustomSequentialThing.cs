using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Orchestration;
using Microsoft.SemanticKernel.Agents.Orchestration.Extensions;
using Microsoft.SemanticKernel.Agents.Orchestration.Sequential;
using Microsoft.SemanticKernel.Agents.Runtime;

namespace _06_AgentFramework;

/// <summary>
/// An orchestration that provides the input message to the first agent
/// and sequentially passes each agent result to the next agent.
/// </summary>
public class CustomSequentialThing<TInput, TOutput> : AgentOrchestration<TInput, TOutput>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SequentialOrchestration{TInput,TOutput}"/> class.
    /// </summary>
    /// <param name="agents">The agents participating in the orchestration.</param>
    public CustomSequentialThing(params Agent[] agents)
        : base(agents)
    {
    }

    /// <inheritdoc />
    protected override async ValueTask StartAsync(IAgentRuntime runtime, TopicId topic, IEnumerable<ChatMessageContent> input, AgentType? entryAgent)
    {
        if (!entryAgent.HasValue)
        {
            throw new ArgumentException("Entry agent is not defined.", nameof(entryAgent));
        }
        await runtime.SendMessageAsync(CustomSequentialMessages.AsRequestMessage(input), entryAgent.Value).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async ValueTask<AgentType?> RegisterOrchestrationAsync(IAgentRuntime runtime, OrchestrationContext context, RegistrationContext registrar, ILogger logger)
    {
        AgentType outputType = await registrar.RegisterResultTypeAsync<CustomSequentialMessages.Response>(Transform).ConfigureAwait(false);

        // Each agent handsoff its result to the next agent.
        AgentType nextAgent = outputType;
        for (int index = this.Members.Count - 1; index >= 0; --index)
        {
            Agent agent = this.Members[index];
            nextAgent = await RegisterAgentAsync(agent, index, nextAgent).ConfigureAwait(false);
        }

        return nextAgent;

        ValueTask<AgentType> RegisterAgentAsync(Agent agent, int index, AgentType nextAgent) =>
            runtime.RegisterAgentFactoryAsync(
                this.GetAgentType(context.Topic, index),
                (agentId, runtime) =>
                {
                    CustomSequentialActor actor = new(agentId, runtime, context, agent, nextAgent, context.LoggerFactory.CreateLogger<CustomSequentialActor>());
#if !NETCOREAPP
                    return actor.AsValueTask<IHostableAgent>();
#else
                    return ValueTask.FromResult<IHostableAgent>(actor);
#endif
                });
    }

    private IList<ChatMessageContent> Transform(CustomSequentialMessages.Response response)
    {
        return [response.Message];
    }

    private AgentType GetAgentType(TopicId topic, int index) => this.FormatAgentType(topic, $"Agent_{index + 1}");
}