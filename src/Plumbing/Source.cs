using System.Diagnostics;

namespace Plumbing;

public static class Source
{
    public const string SourceName = "SemanticKernelDemo";
    public static ActivityContext Current { get; set; }
    private static readonly ActivitySource ASource = new ActivitySource(SourceName, "1.0.0");
    
    public static Activity CreateActivity(string name, ActivityContext context)
    {
        Activity? activity = null;
        try
        {
            activity = ASource.StartActivity(name, ActivityKind.Internal, context);
            return activity ?? throw new Exception("Activity is null");
        }
        catch
        {
            activity?.Dispose();
            throw;
        }
    }
    
    public static void Ok(this Activity activity)
    {
        activity.SetTag("otel.status_code", "OK");
    }
}