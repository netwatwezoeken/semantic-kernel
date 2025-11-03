using Microsoft.SemanticKernel;

namespace _04b_FunctionFilter;

public class NickelbackFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        if (!"Nickelback".Equals(context.Arguments.First(_ => _.Key == "artist").Value.ToString()))
            await next(context);
        
        context.Result = new FunctionResult(context.Result, "I'm sorry Dave, I'm afraid I can't do that.");
    }
}
