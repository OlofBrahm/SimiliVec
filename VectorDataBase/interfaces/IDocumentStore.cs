using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VectorDataBase.Models;

namespace VectorDataBase.Interfaces;

public interface IDocumentStore
{
    Task<IEnumerable<DocumentModel>> LoadAllDocumentsAsync(CancellationToken cancellationToken = default);
    Task SaveAllDocumentsAsync(IEnumerable<DocumentModel> documents, CancellationToken cancellationToken = default);
}
