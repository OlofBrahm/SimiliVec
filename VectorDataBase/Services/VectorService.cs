using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using VectorDataBase.Interfaces;
using VectorDataBase.Core;
using VectorDataBase.Datahandling;
using VectorDataBase.Utils;
using System.Threading.Tasks;
using VectorDataBase.PCA;
using System.Text.Json;
using VectorDataBase.UMAP;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;

namespace VectorDataBase.Services;

public sealed class VectorService : IVectorService
{
    private readonly IDataIndex _dataIndex;
    private readonly IEmbeddingModel _embeddingModel;
    private readonly PCAConversion _pcaConverter;
    private readonly Dictionary<string, DocumentModel> _documentStorage = new();
    private readonly Dictionary<int, string> _indexToDocumentMap = new Dictionary<int, string>();
    private readonly UmapConversion _umapConverter;
    private int _currentId = 0;
    private int NextId() => Interlocked.Increment(ref _currentId);
    private readonly Random _random = Random.Shared;
    private readonly IDataLoader _dataLoader;
    private readonly string _docPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SimiliVec", "documents.json");

    public VectorService(IDataIndex dataIndex, IEmbeddingModel embeddingModel, IDataLoader dataLoader, PCAConversion pcaConverter, UmapConversion umapConverter)
    {
        _dataIndex = dataIndex;
        _embeddingModel = embeddingModel;
        _dataLoader = dataLoader;
        _pcaConverter = pcaConverter;
        _umapConverter = umapConverter;

        _documentStorage = _dataLoader.LoadAllDocuments().ToDictionary(doc => doc.Id, doc => doc);
        IndexDocument();
    }
     

    public async Task<List<UmapNode>> GetUmapNodes()
    {
        //Fixed node order
        var nodesList = _dataIndex.Nodes.Values.ToList();

        var (indices, distances) = _dataIndex.KnnMatrix(15);

        var coords = await _umapConverter.GetUmapProjectionAsync(indices, distances);
        return nodesList.Select((node, i) => new UmapNode
        {
            Id = node.Id,
            X = coords[i][0],
            Y = coords[i][1],
            Content = _documentStorage[_indexToDocumentMap[node.Id]].Content
        }).ToList(); //Can remake so that it is stored in a list from the start to avoid the ToList overhead.
    }

    /// <summary>
    /// Return the HNSW Nodes in the data index
    /// </summary>
    /// <returns></returns>
    public Task<Dictionary<int, PCANode>> GetPCANodes()
    {
        var nodes = _pcaConverter.ConvertToPCA(_dataIndex.Nodes);
        foreach(var node in nodes.Values)
        {
            if(_indexToDocumentMap.TryGetValue(node.Id, out var docId))
            {
                node.DocumentId = docId;

                if(_documentStorage.TryGetValue(docId, out var doc))
                {
                    node.Content = doc.Content;
                }
            }
        }
        return Task.FromResult(nodes);
    }

    /// <summary>
    /// Return the collection of documents in storage
    /// </summary>
    /// <returns></returns>
    public IReadOnlyDictionary<string, DocumentModel> GetDocuments() => _documentStorage;


    /// <summary>
    /// Index documents from a text file
    /// </summary>
    /// <returns></returns>
    public Task IndexDocument()
    {
        const int maxChunkSize = 500;
        int totalChunks = 0;

        foreach (var document in _documentStorage.Values)
        {
            var chunks = SimpleTextChunker.Chunk(document.Content, maxChunkSize);
            foreach (var chunkText in chunks)
            {
                if (string.IsNullOrWhiteSpace(chunkText)) continue; // avoid zero vectors

                var vector = _embeddingModel.GetEmbeddings(chunkText, isQuery: false);
                var nodeId = NextId();
                var node = new HnswNode { Id = nodeId, Vector = vector };

                _dataIndex.Insert(node, _random);
                _indexToDocumentMap[nodeId] = document.Id;
                totalChunks++;
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Search for cloesest k neighbours.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="k"></param>
    /// <returns>SearchResponse</returns>
    public  Task<SearchResponse> Search(string query, int k = 5)
    {
        var queryVector = _embeddingModel.GetEmbeddings(query, isQuery: true);
        var query3D = _pcaConverter.Transform(queryVector);

        var nearestIds = _dataIndex.FindNearestNeighbors(queryVector, k);

        var hits = new List<SearchHit>(nearestIds.Count);
        foreach (var id in nearestIds)
        {
            if (!_dataIndex.Nodes.TryGetValue(id, out var node)) continue;

            float sim = HNSWUtils.CosineSimilarity(queryVector, node.Vector);
            if (float.IsNaN(sim)) sim = 0f; // guard against zero vectors
            float dist = 1.0f - sim;

            if (_indexToDocumentMap.TryGetValue(id, out var docId) &&
                _documentStorage.TryGetValue(docId, out var doc))
            {
                hits.Add(new SearchHit
                {
                    NodeId = id,
                    DocumentId = docId,
                    Similarity = sim,
                    Distance = dist,
                    Document = doc

                });
            }
        }

        hits = hits.OrderByDescending(h => h.Similarity).ToList();

        var seen = new HashSet<string>();
        var orderedDistinctDocs = new List<DocumentModel>();
        foreach (var h in hits)
            if (seen.Add(h.DocumentId)) orderedDistinctDocs.Add(h.Document);

        var response = new SearchResponse
        {
            QueryPosition = query3D,
            Documents = orderedDistinctDocs,
            ResultNodeIds = hits.Select(h => h.NodeId).ToList(),
            Results = hits
        };

        return Task.FromResult(response);
    }

    public async Task AddDocument(DocumentModel doc, bool indexChunks = true)
    {
        // Persist in memory
        _documentStorage[doc.Id] = doc;

        // Write to disk
        Directory.CreateDirectory(Path.GetDirectoryName(_docPath)!);
        var allDocs = _documentStorage.Values.ToList();
        var json = JsonSerializer.Serialize(allDocs, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_docPath, json);

        if (!indexChunks) return;

        // Chunk, embed, and index
        const int maxChars = 1500;
        var chunks = SimpleTextChunker.Chunk(doc.Content, maxChars);
        foreach (var chunkText in chunks)
        {
            if (string.IsNullOrWhiteSpace(chunkText)) continue;

            var vector = _embeddingModel.GetEmbeddings(chunkText, isQuery: false);
            var nodeId = NextId();
            var node = new HnswNode { Id = nodeId, Vector = vector };
            _dataIndex.Insert(node, _random);
            _indexToDocumentMap[nodeId] = doc.Id;
        }
    }
}
public class SearchHit
{
    public int NodeId { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public float Distance { get; set; }          // 1 - similarity
    public float Similarity { get; set; }        // 0..1 cosine similarity
    public DocumentModel Document { get; set; } = default!;
}

public class SearchResponse
{
    public float[] QueryPosition { get; set; } = Array.Empty<float>();
    public List<DocumentModel> Documents { get; set; } = new();
    public List<int> ResultNodeIds { get; set; } = new();
    // New: detailed hits including distances
    public List<SearchHit> Results { get; set; } = new();
}