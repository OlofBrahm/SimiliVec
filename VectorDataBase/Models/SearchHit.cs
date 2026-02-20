namespace VectorDataBase.Models;

/// <summary>
/// Represents a single search result hit with similarity metrics
/// </summary>
public class SearchHit
{
    public int NodeId { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public float Distance { get; set; }          // 1 - similarity
    public float Similarity { get; set; }        // 0..1 cosine similarity
    public DocumentModel Document { get; set; } = default!;
}
