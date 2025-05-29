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

    [VectorStoreKey]
    public string Key { get; set; }

    [VectorStoreData]
    public string? StoryPoints { get; set; }

    [VectorStoreData]
    public string? Description { get; set; }

    [VectorStoreData]
    public string? Title { get; set; }
    
    [VectorStoreVector(1024)]
    public ReadOnlyMemory<float>? Vector { get; set; }
}