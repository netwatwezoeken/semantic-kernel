using Plumbing;

namespace WebUI;

public class DemoSelector
{
    public IDemo SelectedDemo { get; private set; }
    private readonly IEnumerable<IDemo> _demos;

    public DemoSelector(IEnumerable<IDemo> demos)
    {
        _demos = demos;
        SelectedDemo = demos.First();
        SelectedDemo.Start();
    }
    
    public void Start(string name)
    {
        SelectedDemo.Stop();
        if (_demos.Any(d => d.Name == name))
        {
            SelectedDemo = _demos.First(d => d.Name == name);
        }
        else
        {
            SelectedDemo = _demos.First();
        }
        SelectedDemo.Start();
    }
}