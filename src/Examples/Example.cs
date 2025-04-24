using Examples;

public class Example<T>() where T : IExample, new()
{
    public static async Task Run()
    {
        Console.WriteLine($"------ {typeof(T)} ------");
        var example = new T();
        Console.WriteLine(await example.Execute());
    }
}