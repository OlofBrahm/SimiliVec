using System.Collections.Generic;
using System.Threading.Tasks;
using VectorDataBase.Models;

namespace VectorDataBase.Interfaces;

public interface IDocumentRepository
{
    IReadOnlyDictionary<string, DocumentModel> GetAllDocuments();
    bool TryGetDocument(string documentId, out DocumentModel? document);
    bool ContainsDocument(string documentId);
    Task SaveDocumentAsync(DocumentModel doc);
}
