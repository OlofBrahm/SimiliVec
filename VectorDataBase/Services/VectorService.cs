using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using VectorDataBase.Interfaces;
using VectorDataBase.Core;
using VectorDataBase.Datahandling;
using VectorDataBase.Utils;
using System.Threading.Tasks;
using VectorDataBase.PCA;
using Microsoft.VisualBasic;

namespace VectorDataBase.Services;

public class VectorService : IVectorService
{
    private readonly IDataIndex _dataIndex;
    private readonly IEmbeddingModel _embeddingModel;
    private readonly PCAConversion _pcaConverter;
    private readonly Dictionary<string, DocumentModel> _documentStorage = new Dictionary<string, DocumentModel>();
    private readonly Dictionary<int, string> _indexToDocumentMap = new Dictionary<int, string>();
    private int _currentId = 0;
    private int NextId() => Interlocked.Increment(ref _currentId);
    private readonly Random _random = new Random();
    private readonly IDataLoader _dataLoader;
    public VectorService(IDataIndex dataIndex, IEmbeddingModel embeddingModel, IDataLoader dataLoader, PCAConversion pcaConverter)
    {
        Console.WriteLine("VectorService: Constructor called");
        _dataIndex = dataIndex;
        _embeddingModel = embeddingModel;
        _dataLoader = dataLoader;
        _pcaConverter = pcaConverter;
        Console.WriteLine("VectorService: Loading documents...");
        _documentStorage = _dataLoader.LoadAllDocuments().ToDictionary(doc => doc.Id, doc => doc);
        Console.WriteLine($"VectorService: Constructor complete. Loaded {_documentStorage.Count()} documents");
    }


    /// <summary>
    /// Return the HNSW Nodes in the data index
    /// </summary>
    /// <returns></returns>
    public async Task<Dictionary<int, PCANode>> GetPCANodes()
    {
        // Need to be ran through PCA before returning, need to retain Ids
        return await Task.Run(() => _pcaConverter.ConvertToPCA(_dataIndex.Nodes));
    }

    /// <summary>
    /// Return the collection of documents in storage
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, DocumentModel> GetDocuments()
    {
        return _documentStorage;
    }

    /// <summary>
    /// Index documents from a text file
    /// </summary>
    /// <returns></returns>
    public Task IndexDocument()
    {
        int totalChunks = 0;
        Console.WriteLine($"IndexDocument: Starting indexing for {_documentStorage.Count()} documents");

        foreach (var document in _documentStorage.Values)
        {
            string[] chunks = SimpleTextChunker.Chunk(document.Content, maxChunkSize: 500);
            Console.WriteLine($"Document {document.Id}: {chunks.Length} chunks");

            foreach (var chunkText in chunks)
            {
                float[] vector = _embeddingModel.GetEmbeddings(chunkText);
                var nodeId = NextId();
                var node = new HnswNode { id = nodeId, Vector = vector };
                _dataIndex.Insert(node, _random);
                _indexToDocumentMap[nodeId] = document.Id;
                totalChunks++;
            }
        }
        Console.WriteLine($"IndexDocument: Indexing complete. Total {totalChunks} chunks indexed");
        return Task.CompletedTask;
    }

    /// <summary>
    /// returns all documents from storage
    /// </summary>
    /// <returns></returns>
    public IEnumerable<DocumentModel> GetAllDocuments()
    {
        return _documentStorage.Values;
    }

    /// <summary>
    /// Search for cloesest k neighbours.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    public async Task<SearchRespone> Search(string query, int k = 5)
    {
        return await Task.Run(() =>
        {
            float[] queryVector = _embeddingModel.GetEmbeddings(query);
            float[] query3D = _pcaConverter.Transform(queryVector);

            var nearestIds = _dataIndex.FindNearestNeighbors(queryVector, k);
            var results = nearestIds
            .Where(id => _indexToDocumentMap.ContainsKey(id))
            .Select(id => _documentStorage[_indexToDocumentMap[id]])
            .Distinct()
            .ToList();
            return new SearchRespone
            {
                QueryPosition = query3D,
                Documents = results,
                ResultNodeIds = nearestIds
            };
        });
    }



}
public class SearchRespone
{
    public float[] QueryPosition { get; set; } = Array.Empty<float>();
    public List<DocumentModel> Documents { get; set; } = new();
    public List<int> ResultNodeIds { get; set; } = new();
}