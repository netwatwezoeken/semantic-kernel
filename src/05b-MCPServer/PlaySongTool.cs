using System.ComponentModel;
using ModelContextProtocol.Server;

namespace _05b_MCPServer;

[McpServerToolType]
public class PlaySongTool
{
    [McpServerTool, Description("Plays a song.")]
    public static string PlaySong(string artist, string song)
    {
        Console.WriteLine($"Playing {song} by {artist}");
        return $"Playing {song} by {artist}";
    }
}