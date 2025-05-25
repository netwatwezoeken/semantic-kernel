namespace Plumbing;

public class MessageRelay
{
    public event Func<ChatMessage, Task>? OnMessageAsync;
    
    public async Task HandleMessageAsync(ChatMessage message)
    {
        if (OnMessageAsync != null)
        {
            var handlers = OnMessageAsync.GetInvocationList();

            var tasks = handlers.Select(d => ((Func<ChatMessage, Task>)d)(message));
            await Task.WhenAll(tasks);

        }
    }
}