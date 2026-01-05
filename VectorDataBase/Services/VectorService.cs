using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using VectorDataBase.Interfaces;
using VectorDataBase.Core;
using VectorDataBase.Datahandling;
using VectorDataBase.Utils;
using System.Threading.Tasks;

namespace VectorDataBase.Services;

public class VectorService : IVectorService
{
    private readonly IDataIndex _dataIndex;
    private readonly IEmbeddingModel _embeddingModel;
    private readonly Dictionary<string, DocumentModel> _documentStorage = new Dictionary<string, DocumentModel>();
    private readonly Dictionary<int, string> _indexToDocumentMap = new Dictionary<int, string>();
    private int _currentId = 0;
    private int NextId() => Interlocked.Increment(ref _currentId);
    private readonly Random _random = new Random();
    private readonly IDataLoader _dataLoader;
    public VectorService(IDataIndex dataIndex, IEmbeddingModel embeddingModel, IDataLoader dataLoader)
    {
        Console.WriteLine("VectorService: Constructor called");
        _dataIndex = dataIndex;
        _embeddingModel = embeddingModel;
        _dataLoader = dataLoader;
        Console.WriteLine("VectorService: Loading documents...");
        _documentStorage = _dataLoader.LoadAllDocuments().ToDictionary(doc => doc.Id, doc => doc);
        Console.WriteLine($"VectorService: Constructor complete. Loaded {_documentStorage.Count()} documents");
    }

    /// <summary>
    /// Index documents from a text file
    /// </summary>
    /// <returns></returns>
    public Task IndexDocument()
    {
        int totalChunks = 0;
        Console.WriteLine($"IndexDocument: Starting indexing for {_documentStorage.Count()} documents");
        
        foreach(var document in _documentStorage.Values)
        {
            string[] chunks = SimpleTextChunker.Chunk(document.Content, maxChunkSize: 500);
            Console.WriteLine($"Document {document.Id}: {chunks.Length} chunks");

            foreach(var chunkText in chunks)
            {
                float[] vector = _embeddingModel.GetEmbeddings(chunkText);
                var nodeId = NextId();
                var node = new HsnwNode { id = nodeId, Vector = vector };
                _dataIndex.Insert(node, _random);
                _indexToDocumentMap.Add(nodeId, document.Id);
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
    public Task<IEnumerable<DocumentModel>> Search(string query, int k = 5)
    {
        Console.WriteLine($"Search: Query='{query}', k={k}");
        //Embedd the search query
        float[] queryVector = _embeddingModel.GetEmbeddings(query);
        Console.WriteLine($"Search: Generated query vector with {queryVector.Length} dimensions");
        //Find the nearest k neighors to the query
        var nearestVectorId = _dataIndex.FindNearestNeighbors(queryVector, k);
        Console.WriteLine($"Search: Found {nearestVectorId.Count()} nearest neighbors");
        
        var foundDocumentIds = new HashSet<string>();
        var results = new List<DocumentModel>();

        foreach(var vectorId in nearestVectorId)
        {
            Console.WriteLine($"Search: Checking vectorId={vectorId}");
            if(_indexToDocumentMap.TryGetValue(vectorId, out string? documentId))
            {
                Console.WriteLine($"Search: Found document mapping: vectorId={vectorId} -> documentId={documentId}");
                if(foundDocumentIds.Add(documentId))
                {
                    if(_documentStorage.TryGetValue(documentId, out DocumentModel? fullDocument))
                    {
                        var vector = _dataIndex.Nodes[vectorId].Vector;
                        fullDocument.Distance = HNSWUtils.CosineSimilarity(queryVector, vector);
                        results.Add(fullDocument);
                        Console.WriteLine($"Search: Added document {documentId} with distance {fullDocument.Distance}");
                    }
                    else
                    {
                        Console.WriteLine($"Search: Document {documentId} not found in storage");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Search: No mapping found for vectorId={vectorId}");
            }
        }
        
        Console.WriteLine($"Search: Returning {results.Count} results");
        _dataLoader.SaveDataToFile(_documentStorage.Values);
        return Task.FromResult<IEnumerable<DocumentModel>>(results);
    }

}