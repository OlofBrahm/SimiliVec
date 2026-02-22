using System;
using System.Collections.Generic;
using System.Linq;
using VectorDataBase.Interfaces;
using VectorDataBase.Core;
using VectorDataBase.Models;
using VectorDataBase.Utils;
using System.Threading.Tasks;
using VectorDataBase.DimensionalityReduction.PCA;
using VectorDataBase.DimensionalityReduction.UMAP;
using VectorDataBase.Repositories;
using System.Numerics.Tensors;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualBasic;
using System.Diagnostics.CodeAnalysis;

namespace VectorDataBase.Services;

public sealed class VectorService : IVectorService
{
    private readonly IDataIndex _dataIndex;
    private readonly IEmbeddingModel _embeddingModel;
    private readonly PCAConversion _pcaConverter;
    private readonly UmapConversion _umapConverter;
    private readonly DocumentRepository _documentRepository;
    private readonly NodeDocumentMapper _nodeMapper;
    private readonly CoordinateNormalizer _normalizer;
    private readonly Random _random = Random.Shared;
    private readonly Dictionary<int, float[]> _umapCoordinateCache = new();
    private readonly Dictionary<int, float[]> _pcaCoordinateCache = new();

    private CoordinateNormalizer.NormalizationParams? _umapNormParams;
    private CoordinateNormalizer.NormalizationParams? _pcaNormParams;

    public VectorService(
        IDataIndex dataIndex,
        IEmbeddingModel embeddingModel,
        PCAConversion pcaConverter,
        UmapConversion umapConverter,
        DocumentRepository documentRepository,
        NodeDocumentMapper nodeMapper,
        CoordinateNormalizer normalizer)
    {
        _dataIndex = dataIndex;
        _embeddingModel = embeddingModel;
        _pcaConverter = pcaConverter;
        _umapConverter = umapConverter;
        _documentRepository = documentRepository;
        _nodeMapper = nodeMapper;
        _normalizer = normalizer;

        IndexDocument();
    }

    /// <summary>
    /// Returns the index with reduced dimensions using UMAP.
    /// </summary>
    /// <returns></returns>
    public async Task<List<UmapNode>> GetUmapNodes()
    {
        var nodesList = _dataIndex.Nodes.Values.OrderBy(n => n.Id).ToList();

        if (nodesList.Count == 0) return new List<UmapNode>();

        var coords = await _umapConverter.GetUmapProjectionAsync(_dataIndex.Nodes);

        //Normalize 3D space
        var (normalized, normParams) = _normalizer.Normalize3D(coords);
        this._umapNormParams = normParams;

        _umapCoordinateCache.Clear();

        return nodesList.Select((node, i) =>
        {
            var vec = new[] { normalized[i].x, normalized[i].y, normalized[i].z };
            _umapCoordinateCache[node.Id] = vec;

            if (_nodeMapper.TryGetDocumentId(node.Id, out var docId) &&
                docId != null &&
                _documentRepository.TryGetDocument(docId, out var doc) &&
                doc != null)
            {
                return new UmapNode
                {
                    Id = node.Id,
                    DocumentId = doc.Id,
                    Content = doc.Content,
                    ReducedVector = vec
                };
            }

            return new UmapNode
            {
                Id = node.Id,
                DocumentId = "",
                Content = "",
                ReducedVector = vec
            };
        }).ToList();
    }

    /// <summary>
    /// Returns the index with reduced dimensions using UMAP.
    /// </summary>
    /// <returns></returns>
    public async Task<List<UmapNode>> GetUmapCalculatedNodes()
    {
        var nodesList = _dataIndex.Nodes.Values.OrderBy(n => n.Id).ToList();

        if (nodesList.Count == 0) return new List<UmapNode>();

        var coords = await _umapConverter.GetUmapNodesAsync();

        //Normalize 3D space
        var (normalized, normParams) = _normalizer.Normalize3D(coords);
        this._umapNormParams = normParams;

        _umapCoordinateCache.Clear();

        return nodesList.Select((node, i) =>
        {
            var vec = new[] { normalized[i].x, normalized[i].y, normalized[i].z };
            _umapCoordinateCache[node.Id] = vec;

            if (_nodeMapper.TryGetDocumentId(node.Id, out var docId) &&
                docId != null &&
                _documentRepository.TryGetDocument(docId, out var doc) &&
                doc != null)
            {
                return new UmapNode
                {
                    Id = node.Id,
                    DocumentId = doc.Id,
                    Content = doc.Content,
                    ReducedVector = vec
                };
            }

            return new UmapNode
            {
                Id = node.Id,
                DocumentId = "",
                Content = "",
                ReducedVector = vec
            };
        }).ToList();
    }

    /// <summary>
    /// Return the index with reduced dimensions using PCA.
    /// </summary>
    /// <returns></returns>
    public Task<Dictionary<int, PCANode>> GetPCANodes()
    {
        var nodes = _pcaConverter.ConvertToPCA(_dataIndex.Nodes);

        _pcaNormParams = _normalizer.NormalizePcaNodes(nodes);
        foreach (var node in nodes.Values)
        {
            _pcaCoordinateCache[node.Id] = new[] { node.ReducedVector[0], node.ReducedVector[1], node.ReducedVector[2] };
            if (_nodeMapper.TryGetDocumentId(node.Id, out var docId) && docId != null)
            {
                node.DocumentId = docId;

                if (_documentRepository.TryGetDocument(docId, out var doc) && doc != null)
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
    public IReadOnlyDictionary<string, DocumentModel> GetDocuments() => _documentRepository.GetAllDocuments();


    /// <summary>
    /// Index documents from a text file
    /// </summary>
    /// <returns></returns>
    public Task IndexDocument()
    {
        const int maxChunkSize = 500;
        int totalChunks = 0;

        foreach (var document in _documentRepository.GetAllDocuments().Values)
        {
            var chunks = SimpleTextChunker.Chunk(document.Content, maxChunkSize);
            foreach (var chunkText in chunks)
            {
                if (string.IsNullOrWhiteSpace(chunkText)) continue; // avoid zero vectors

                var vector = _embeddingModel.GetEmbeddings(chunkText, isQuery: false);
                var nodeId = _nodeMapper.NextId();
                var node = new HnswNode { Id = nodeId, Vector = vector };

                _dataIndex.Insert(node, _random);
                _nodeMapper.MapNodeToDocument(nodeId, document.Id);
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
    public Task<SearchResponse> Search(string query, int k = 5)
    {
        var queryVector = _embeddingModel.GetEmbeddings(query, isQuery: true);
        var nearestIds = _dataIndex.FindNearestNeighbors(queryVector, k);

        var hits = new List<SearchHit>(nearestIds.Count);

        foreach (var id in nearestIds)
        {
            if (!_dataIndex.Nodes.TryGetValue(id, out var node)) continue;

            float sim = HNSWUtils.CosineSimilarity(queryVector, node.Vector);
            if (float.IsNaN(sim)) sim = 0f;
            float dist = 1.0f - sim;

            if (_nodeMapper.TryGetDocumentId(id, out var docId) &&
            docId != null &&
            _documentRepository.TryGetDocument(docId, out var doc) &&
            doc != null)
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
        float[] query3D = CalculateVisualPosition(hits, _pcaCoordinateCache);

        var bestHits = hits
        .GroupBy(h => h.DocumentId)
        .Select(g => g.OrderByDescending(h => h.Similarity).First())
        .OrderByDescending(h => h.Similarity)
        .ToList();

        return Task.FromResult(new SearchResponse
        {
            QueryPosition = query3D,
            Documents = bestHits.Select(h => h.Document).ToList(),
            ResultNodeIds = bestHits.Select(h => h.NodeId).ToList(),
            Results = bestHits
        });
    }

    /// <summary>
    /// Adds a document to the network, and will also persist the document.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="indexChunks"></param>
    /// <returns></returns>
    public async Task AddDocument(DocumentModel doc, bool indexChunks = true)
    {
        // Persist document
        //await _documentRepository.SaveDocumentAsync(doc);

        if (!indexChunks) return;

        // Chunk, embed, and index
        const int maxChars = 1500;
        var chunks = SimpleTextChunker.Chunk(doc.Content, maxChars);
        foreach (var chunkText in chunks)
        {
            if (string.IsNullOrWhiteSpace(chunkText)) continue;

            var vector = _embeddingModel.GetEmbeddings(chunkText, isQuery: false);
            var nodeId = _nodeMapper.NextId();
            var node = new HnswNode { Id = nodeId, Vector = vector };
            _dataIndex.Insert(node, _random);
            _nodeMapper.MapNodeToDocument(nodeId, doc.Id);

            // CRITICAL: Invalidate UMAP cache since we have new nodes
            _umapCoordinateCache.Clear();
            _umapNormParams = null;
        }
    }

    /// <summary>
    /// Search through UMAP, if a node is a 100% hit it will give the query the coordinates 
    /// of that node.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    public async Task<SearchResponse> SearchUmap(string query, int k = 5)
    {
        // Check if UMAP cache is empty - retrain if needed
        if (_umapCoordinateCache.Count == 0)
        {
            Console.WriteLine("[SearchUmap] Cache empty, retraining UMAP...");
            await GetUmapCalculatedNodes(); // This rebuilds the cache
        }
        
        var queryVector = _embeddingModel.GetEmbeddings(query, isQuery: true);

        // Find nearest neighbors first
        var nearestIds = _dataIndex.FindNearestNeighbors(queryVector, k);
        var hits = new List<SearchHit>(nearestIds.Count);

        foreach (var id in nearestIds)
        {
            if (!_dataIndex.Nodes.TryGetValue(id, out var node)) continue;

            float sim = HNSWUtils.CosineSimilarity(queryVector, node.Vector);
            if (float.IsNaN(sim)) sim = 0f;
            float dist = 1.0f - sim;

            if (_nodeMapper.TryGetDocumentId(id, out var docId) &&
                docId != null &&
                _documentRepository.TryGetDocument(docId, out var doc) &&
                doc != null)
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

        // Find the most accurate 3d coordinate based on the closest K
        float[] query3D = CalculateVisualPosition(hits, _umapCoordinateCache);

        var bestHits = hits
        .GroupBy(h => h.DocumentId)
        .Select(g => g.OrderByDescending(h => h.Similarity).First())
        .OrderByDescending(h => h.Similarity)
        .ToList();

        return new SearchResponse
        {
            QueryPosition = query3D,
            Documents = bestHits.Select(h => h.Document).ToList(),
            ResultNodeIds = bestHits.Select(h => h.NodeId).ToList(),
            Results = bestHits
        };
    }

    private float[] CalculateVisualPosition(List<SearchHit> hits, Dictionary<int, float[]> cache, int power = 2)
    {
        if (hits == null || !hits.Any()) return new float[] { 0, 0, 0 };

        // Filter hits to only those present in the cache to avoid KeyNotFoundException
        var validHits = hits.Where(h => cache.ContainsKey(h.NodeId)).ToList();
        if (!validHits.Any()) return new float[] { 0, 0, 0 };

        double totalWeight = 0;
        double[] weightedPos = new double[3];

        foreach (var hit in validHits)
        {
            // Use Distance (1 - Similarity). Small epsilon avoids division by zero.
            double distance = Math.Max(hit.Distance, 1e-6);
            double weight = 1.0 / Math.Pow(distance, power);

            var coords = cache[hit.NodeId];
            weightedPos[0] += coords[0] * weight;
            weightedPos[1] += coords[1] * weight;
            weightedPos[2] += coords[2] * weight;
            totalWeight += weight;
        }

        return new float[]
        {
        (float)(weightedPos[0] / totalWeight),
        (float)(weightedPos[1] / totalWeight),
        (float)(weightedPos[2] / totalWeight)
        };
    }
}