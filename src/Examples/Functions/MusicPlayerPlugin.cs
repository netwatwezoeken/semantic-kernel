using Microsoft.SemanticKernel;

namespace Examples.Functions;

public class MusicPlayerPlugin
{
    [KernelFunction("play_song")]
    public void PlaySong(string artist, string song)
    {
        Console.WriteLine($"MusicPlayerPlugin: Playing {song} by {artist}");
    }
}