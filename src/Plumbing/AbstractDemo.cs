namespace Plumbing;

public abstract class AbstractDemo : IDemo
{
    public string Name { get; protected init; } = null!;
    public string DemoQuestion { get; protected init; } = null!;
    public string Instruction  { get; protected init; } = null!;

    protected readonly MessageRelay Relay;

    protected AbstractDemo(MessageRelay relay)
    {
        Relay = relay;
    }

    public virtual async Task Start()
    {
        await BeforeStart();
        Relay.OnMessageAsync += HandelMessage;
        await Relay.HandleMessageAsync(
            new ChatMessage
            {
                From = Role.Instructor,
                Message = Instruction
            });
    }

    protected virtual Task BeforeStart()
    {
        return Task.CompletedTask;
    }

    protected abstract Task<string> OnHandleUserMessage(ChatMessage message);

    protected virtual async Task HandelMessage(ChatMessage message)
    {
        if (message.From != "user") return;

        var result = await OnHandleUserMessage(message);
        
        await Relay.HandleMessageAsync(
            new ChatMessage
            {
                From = "assistant",
                Message = result
            });
    }

    public virtual Task Stop()
    {
        Relay.OnMessageAsync -= HandelMessage;
        return Task.CompletedTask;
    }
}