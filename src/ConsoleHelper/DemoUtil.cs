namespace SharedStuff;

public static class DemoUtil
{
    public static string SimulateTyping(string input)
    {
        Console.Write("You: ");
        foreach (var c in input)
        {
            Console.Write(c);
            Thread.Sleep(30);
        }

        Console.WriteLine("");

        return input;
    }
}