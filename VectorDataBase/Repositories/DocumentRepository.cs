using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using VectorDataBase.Models;
using VectorDataBase.Interfaces;

namespace VectorDataBase.Repositories;

/// <summary>
/// Manages document storage, persistence, and retrieval
/// </summary>
public sealed class DocumentRepository : IDocumentRepository, IDisposable
{
    private ConcurrentDictionary<string, DocumentModel> _documentStorage = new();
    private readonly IDocumentStore _documentStore;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private bool _disposed = false;

    /// <summary>
    /// Private constructor for internal use only. Use CreateAsync factory method for initialization.
    /// </summary>
    private DocumentRepository(IDocumentStore documentStore)
    {
        _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
    }

    /// <summary>
    /// Creates and asynchronously initializes a new DocumentRepository instance.
    /// This avoids blocking on async operations and prevents potential deadlocks.
    /// </summary>
    public static async Task<DocumentRepository> CreateAsync(IDocumentStore documentStore)
    {
        if (documentStore is null)
        {
            throw new ArgumentNullException(nameof(documentStore));
        }

        var repository = new DocumentRepository(documentStore);
        await repository.InitializeAsync();
        return repository;
    }

    /// <summary>
    /// Asynchronously initializes the repository by loading and validating documents.
    /// </summary>
    private async Task InitializeAsync()
    {
        var loadedDocuments = (await _documentStore.LoadAllDocumentsAsync())?.ToList() ?? new List<DocumentModel>();
        var invalidDocs = loadedDocuments.Where(doc => string.IsNullOrWhiteSpace(doc.Id)).ToList();
        if (invalidDocs.Count > 0)
        {
            throw new InvalidOperationException($"Found {invalidDocs.Count} document(s) with null or empty IDs.");
        }

        var duplicateIds = loadedDocuments
            .GroupBy(doc => doc.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateIds.Count > 0)
        {
            throw new InvalidOperationException($"Duplicate document IDs detected while loading documents: {string.Join(", ", duplicateIds)}");
        }

        _documentStorage = new ConcurrentDictionary<string, DocumentModel>(loadedDocuments.ToDictionary(doc => doc.Id, doc => doc));
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
        await SaveDocumentAsync(doc, CancellationToken.None);
    }

    /// <summary>
    /// Add or update a document and persist to disk with cancellation support.
    /// </summary>
    private async Task SaveDocumentAsync(DocumentModel doc, CancellationToken cancellationToken)
    {
        if (doc is null)
        {
            throw new ArgumentNullException(nameof(doc));
        }

        if (string.IsNullOrWhiteSpace(doc.Id))
        {
            throw new ArgumentException("Document ID cannot be null, empty, or whitespace.", nameof(doc));
        }

        await _saveLock.WaitAsync(cancellationToken);
        try
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(DocumentRepository));
            }

            var documentsToPersist = new Dictionary<string, DocumentModel>(_documentStorage)
            {
                [doc.Id] = doc
            };

            await _documentStore.SaveAllDocumentsAsync(documentsToPersist.Values, cancellationToken);
            _documentStorage[doc.Id] = doc;
        }
        finally
        {
            _saveLock.Release();
        }
    }
    /// <summary>
    /// Check if a document exists
    /// </summary>
    public bool ContainsDocument(string documentId) => _documentStorage.ContainsKey(documentId);

    /// <summary>
    /// Disposes the repository and releases the SemaphoreSlim resource.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _saveLock?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
