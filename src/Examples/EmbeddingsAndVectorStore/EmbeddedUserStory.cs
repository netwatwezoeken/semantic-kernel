using Microsoft.Extensions.VectorData;

namespace Examples.EmbeddingsAndVectorStore;

public class EmbeddedUserStory
{
    public EmbeddedUserStory()
    {
        
    }
    
    public EmbeddedUserStory(UserStory story)
    {
        Key = story.issuekey;
        StoryPoints = story.storypoints;
        Description = story.description;
        Title = story.title;
    }

    [VectorStoreRecordKey]
    public string Key { get; set; }

    [VectorStoreRecordData]
    public string? StoryPoints { get; set; }

    [VectorStoreRecordData]
    public string? Description { get; set; }

    [VectorStoreRecordData]
    public string? Title { get; set; }

    [VectorStoreRecordVector(1024)]
    public ReadOnlyMemory<float>? Vector { get; set; }
}