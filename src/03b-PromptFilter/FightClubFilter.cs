using Microsoft.SemanticKernel;
using Plumbing;

namespace _03b_PromptFilter;

public class FightClubFilter : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        await next(context);

        var prompt = context.RenderedPrompt;
        if (prompt !=null && 
            prompt.Contains("Fight Club", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new ContentException("We don't talk about Fight Club.");       
        }
    }
}