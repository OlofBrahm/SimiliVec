using System;

namespace VectorDataBase.Models;

/// <summary>
/// Represents a node with UMAP-reduced coordinates
/// </summary>
public class UmapNode
{
    public int Id { get; set; }
    public required string DocumentId { get; set; }
    public required string Content { get; set; }
    public float[] ReducedVector { get; set; } = Array.Empty<float>();
}
