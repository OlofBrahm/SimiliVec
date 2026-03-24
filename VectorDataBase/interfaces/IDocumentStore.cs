using System.Collections.Generic;
using System.Threading.Tasks;
using VectorDataBase.Models;

namespace VectorDataBase.Interfaces;

public interface IDocumentStore
{
    IEnumerable<DocumentModel> LoadAllDocuments();
    Task SaveAllDocumentsAsync(IEnumerable<DocumentModel> documents);
}
