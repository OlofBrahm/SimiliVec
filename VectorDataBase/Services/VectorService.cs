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


    public async Task<List<UmapNode>> GetUmapNodes()
    {
        var nodesList = _dataIndex.Nodes.Values.ToList();
        var vectors = nodesList.Select(n => n.Vector).ToArray();

        var coords = await _umapConverter.FitAndProjectAsync(vectors);

        var (normalized, normParams) = _normalizer.Normalize3D(coords);
        _umapNormParams = normParams;

        return nodesList.Select((node, i) =>
        {
            var vec = new[] { normalized[i].x, normalized[i].y, normalized[i].z };
            
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
    /// Return the HNSW Nodes in the data index
    /// </summary>
    /// <returns></returns>
    public Task<Dictionary<int, PCANode>> GetPCANodes()
    {
        var nodes = _pcaConverter.ConvertToPCA(_dataIndex.Nodes);

        _pcaNormParams = _normalizer.NormalizePcaNodes(nodes);

        foreach (var node in nodes.Values)
        {
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
        var rawQuery3D = _pcaConverter.Transform(queryVector);

        float[] query3D;
        if (_pcaNormParams != null)
        {
            query3D = _normalizer.ApplyNormalization(rawQuery3D, _pcaNormParams);
        }
        else
        {
            query3D = rawQuery3D;
        }

        var nearestIds = _dataIndex.FindNearestNeighbors(queryVector, k);

        var hits = new List<SearchHit>(nearestIds.Count);
        foreach (var id in nearestIds)
        {
            if (!_dataIndex.Nodes.TryGetValue(id, out var node)) continue;

            float sim = HNSWUtils.CosineSimilarity(queryVector, node.Vector);
            if (float.IsNaN(sim)) sim = 0f; // guard against zero vectors
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

        // Group by document, take best chunk per document
        var bestHits = hits
            .GroupBy(h => h.DocumentId)
            .Select(g => g.OrderByDescending(h => h.Similarity).First())
            .OrderByDescending(h => h.Similarity)
            .ToList();

        var orderedDistinctDocs = bestHits.Select(h => h.Document).ToList();

        return Task.FromResult(new SearchResponse
        {
            QueryPosition = query3D,
            Documents = orderedDistinctDocs,
            ResultNodeIds = bestHits.Select(h => h.NodeId).ToList(),
            Results = bestHits
        });
    }

    public async Task AddDocument(DocumentModel doc, bool indexChunks = true)
    {
        // Persist document
        await _documentRepository.SaveDocumentAsync(doc);

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
        }
    }

    public async Task<SearchResponse> SearchUmap(string query, int k = 5)
    {
        var queryVector = _embeddingModel.GetEmbeddings(query, isQuery: true);
        var rawQuery3D = await _umapConverter.TransformQueryAsync(queryVector);

        float[] query3D;
        if (_umapNormParams != null)
        {
            query3D = _normalizer.ApplyNormalization(rawQuery3D, _umapNormParams);
        }
        else
        {
            query3D = rawQuery3D;
        }

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

        // Group by document, take best chunk per document
        var bestHits = hits
            .GroupBy(h => h.DocumentId)
            .Select(g => g.OrderByDescending(h => h.Similarity).First())
            .OrderByDescending(h => h.Similarity)
            .ToList();

        var orderedDistinctDocs = bestHits.Select(h => h.Document).ToList();

        return new SearchResponse
        {
            QueryPosition = query3D,
            Documents = orderedDistinctDocs,
            ResultNodeIds = bestHits.Select(h => h.NodeId).ToList(),
            Results = bestHits
        };
    }
}