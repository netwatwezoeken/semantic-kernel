namespace Plumbing;

public interface IDemo
{
    public string Name { get; }
    public string[] Models { get; }
    public string? DemoQuestion { get; }
    Task Start();
    Task Stop();
}