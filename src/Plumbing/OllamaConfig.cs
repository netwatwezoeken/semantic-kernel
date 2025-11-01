namespace Plumbing;

public class OllamaConfig
{
    public Uri Uri  => Environment.GetEnvironmentVariable("Ollama_Url") == null
        ? new Uri("http://localhost:11434")
        : new Uri(Environment.GetEnvironmentVariable("Ollama_Url"));
}
