using Microsoft.SemanticKernel;
using Plumbing;

namespace _04_Functions;

public class MusicPlayerPlugin
{
    [KernelFunction("play_song")]
    public void PlaySong(string artist, string song)
    {
        using var activity = Source.CreateActivity("play song", Source.Current);
        activity?.SetTag("artist", artist);
        activity?.SetTag("song", song);
        
        Console.WriteLine($"MusicPlayerPlugin: Playing {song} by {artist}");
        Thread.Sleep(100);
        
        activity?.Ok();
    }
}