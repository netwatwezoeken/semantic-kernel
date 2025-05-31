namespace Plumbing;

public class ChatMessage
{
    public required string From { get; set; }
    public required string Message { get; set; }
}

public static class Role
{
    public const string Instructor = "instructor";
}