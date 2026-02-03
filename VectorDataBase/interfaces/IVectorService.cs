using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VectorDataBase.Services;
using VectorDataBase.Datahandling;
using VectorDataBase.PCA;

public interface IVectorService
{
    Task IndexDocument();
    IEnumerable<DocumentModel> GetAllDocuments();
    Task<SearchRespone> Search(string query, int k = 5);
    Dictionary<string, DocumentModel> GetDocuments();
    Task<Dictionary<int, PCANode>> GetPCANodes();
}