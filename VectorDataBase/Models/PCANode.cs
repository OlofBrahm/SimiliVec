using System;

namespace VectorDataBase.Models;

/// <summary>
/// Represents a node with PCA-reduced coordinates
/// </summary>
public class PCANode
{
    public int Id { get; set; }
    public float[] ReducedVector { get; set; } = Array.Empty<float>();
    public string DocumentId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
