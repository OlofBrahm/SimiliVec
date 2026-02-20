using System.Collections.Generic;
using System.Threading;

namespace VectorDataBase.Repositories;

/// <summary>
/// Manages node ID generation and node-to-document mapping
/// </summary>
public sealed class NodeDocumentMapper
{
    private readonly Dictionary<int, string> _indexToDocumentMap = new();
    private int _currentId = 0;

    /// <summary>
    /// Generate the next unique node ID
    /// </summary>
    public int NextId() => Interlocked.Increment(ref _currentId);

    /// <summary>
    /// Map a node ID to a document ID
    /// </summary>
    public void MapNodeToDocument(int nodeId, string documentId)
    {
        _indexToDocumentMap[nodeId] = documentId;
    }

    /// <summary>
    /// Try to get the document ID for a node ID
    /// </summary>
    public bool TryGetDocumentId(int nodeId, out string? documentId)
    {
        return _indexToDocumentMap.TryGetValue(nodeId, out documentId);
    }

    /// <summary>
    /// Get all node-to-document mappings
    /// </summary>
    public IReadOnlyDictionary<int, string> GetAllMappings() => _indexToDocumentMap;
}
