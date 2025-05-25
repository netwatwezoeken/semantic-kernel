using Plumbing;

namespace SharedStuff;

public class ConsoleUi
{
    private readonly MessageRelay _relay;

    public ConsoleUi(MessageRelay relay)
    {
        _relay = relay;
        _relay.OnMessageAsync += async (message) =>
        {
            if (message.From == "user") return;
            Console.WriteLine($"{message.From}: {message.Message}");
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
                Message = question
            });
        }
    }
}