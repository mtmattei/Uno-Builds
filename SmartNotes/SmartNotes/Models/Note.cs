using LiteDB;

namespace SmartNotes.Models;
public class Note
{
    [BsonId]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public float[]? Embedding { get; set; }
}
