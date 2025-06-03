using Plumbing;

namespace SharedStuff;

public class ConsoleUi
{
    private readonly MessageRelay _relay;

    public ConsoleUi(MessageRelay relay)
    {
        _relay = relay;
        _relay.OnMessageAsync += (message) =>
        {
            if (message.From == "user") return Task.CompletedTask;
            Console.WriteLine($"{message.From}: {message.Message}");
            return Task.CompletedTask;
        };
    }
    
    public async Task Run(string initialQuestion)
    {
        DemoUtil.SimulateTyping(initialQuestion);
        //var initialQuestion = DemoUtil.SimulateTyping("Which band invented metal? Just give the band name, no explanation.");
        await _relay.HandleMessageAsync(new ChatMessage()
        {
            From = "user",
            Message = initialQuestion
        });
        while (true)
        {
            Console.Write("You: ");
            var question = Console.ReadLine();
            await _relay.HandleMessageAsync(new ChatMessage()
            {
                From = "user",
                Message = question ?? ""
            });
        }
    }
}