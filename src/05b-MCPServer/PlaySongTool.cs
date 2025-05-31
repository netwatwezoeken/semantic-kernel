using System.ComponentModel;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace _05b_MCPServer;

[McpServerToolType]
public class PlaySongTool(ILogger<PlaySongTool> logger)
{
    [McpServerTool, Description("Plays a song.")]
    public string PlaySong(string artist, string song)
    {
        Thread.Sleep(100);
        logger.LogWarning($"Playing {song} by {artist}");
        return $"Playing {song} by {artist}";
    }
}