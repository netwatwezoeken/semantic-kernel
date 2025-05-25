using _01_Basic;
using Microsoft.AspNetCore.SignalR;
using Plumbing;

namespace WebUI;

public class ChatHub : Hub
{
    private readonly MessageRelay _relay;
    private readonly IHubContext<ChatHub> _context;
    private readonly DemoSelector _demoSelector;

    public ChatHub(MessageRelay relay, DemoSelector demoSelector, IHubContext<ChatHub> context)
    {
        _relay = relay;
        _context = context;
        _demoSelector = demoSelector;
        _relay.OnMessageAsync += Method;
    }
    
    public async Task SendMessage(string user, string message)
    {
        await _relay.HandleMessageAsync(new ChatMessage()
        {
            From = "user",
            Message = message
        });
    }
    
    public async Task Select(string demo)
    {
        _demoSelector.Start(demo);
        await _context.Clients.All.SendAsync("NewDefaultUserMessage", _demoSelector.SelectedDemo.DemoQuestion);
    }

    protected override void Dispose(bool disposing)
    {
        _relay.OnMessageAsync -= Method;
        base.Dispose(disposing);
    }

    private async Task Method(ChatMessage message)
    {
        await _context.Clients.All.SendAsync("ReceiveMessage", message.From, message.Message);
    }
}