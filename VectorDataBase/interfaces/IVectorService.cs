using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VectorDataBase.Services;
using VectorDataBase.Datahandling;
using VectorDataBase.PCA;

public interface IVectorService
{
    Task IndexDocument();
    Task<SearchResponse> Search(string query, int k = 5);
    Task<SearchResponse> SearchUmap(string query, int k = 5);
    IReadOnlyDictionary<string, DocumentModel> GetDocuments();
    Task<Dictionary<int, PCANode>> GetPCANodes();
    Task AddDocument(DocumentModel doc, bool indexChunks = true);
    Task<List<UmapNode>> GetUmapNodes();
}