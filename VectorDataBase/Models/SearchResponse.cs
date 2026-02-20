using System;
using System.Collections.Generic;

namespace VectorDataBase.Models;

/// <summary>
/// Response model containing search results and query position
/// </summary>
public class SearchResponse
{
    public float[] QueryPosition { get; set; } = Array.Empty<float>();
    public List<DocumentModel> Documents { get; set; } = new();
    public List<int> ResultNodeIds { get; set; } = new();
    /// <summary>
    /// Detailed hits including distances and similarity scores
    /// </summary>
    public List<SearchHit> Results { get; set; } = new();
}
