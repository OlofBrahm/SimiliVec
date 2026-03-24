using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VectorDataBase.Models;
using VectorDataBase.Interfaces;

namespace VectorDataBase.Repositories;

/// <summary>
/// Manages document storage, persistence, and retrieval
/// </summary>
public sealed class DocumentRepository : IDocumentRepository
{
    private readonly Dictionary<string, DocumentModel> _documentStorage;
    private readonly IDocumentStore _documentStore;

    public DocumentRepository(IDocumentStore documentStore)
    {
        _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));

        var loadedDocuments = _documentStore.LoadAllDocuments()?.ToList() ?? new List<DocumentModel>();
        var duplicateIds = loadedDocuments
            .GroupBy(doc => doc.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateIds.Count > 0)
        {
            throw new InvalidOperationException($"Duplicate document IDs detected while loading documents: {string.Join(", ", duplicateIds)}");
        }

        _documentStorage = loadedDocuments.ToDictionary(doc => doc.Id, doc => doc);
    }

    /// <summary>
    /// Get all documents in storage
    /// </summary>
    public IReadOnlyDictionary<string, DocumentModel> GetAllDocuments() => _documentStorage;

    /// <summary>
    /// Try to get a document by ID
    /// </summary>
    public bool TryGetDocument(string documentId, out DocumentModel? document)
    {
        return _documentStorage.TryGetValue(documentId, out document);
    }

    /// <summary>
    /// Add or update a document and persist to disk.
    ///  This function is expensive but it works for smaller datasets. Implement smaller increment saving for larger production.
    /// </summary>
    public async Task SaveDocumentAsync(DocumentModel doc)
    {
        _documentStorage[doc.Id] = doc;
        await _documentStore.SaveAllDocumentsAsync(_documentStorage.Values);
    }

    /// <summary>
    /// Check if a document exists
    /// </summary>
    public bool ContainsDocument(string documentId) => _documentStorage.ContainsKey(documentId);
}
