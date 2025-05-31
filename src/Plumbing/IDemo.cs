namespace Plumbing;

public interface IDemo
{
    public string Name { get; }
    public string? DemoQuestion { get; }
    public string Instruction  { get; }
    Task Start();
    Task Stop();
}