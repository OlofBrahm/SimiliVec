using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VectorDataBase.Models;
using VectorDataBase.Interfaces;

namespace VectorDataBase.Repositories;

/// <summary>
/// Manages document storage, persistence, and retrieval
/// </summary>
public sealed class DocumentRepository
{
    private readonly Dictionary<string, DocumentModel> _documentStorage = new();
    private readonly string _docPath;

    public DocumentRepository(IDataLoader dataLoader)
    {
        _docPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SimiliVec", "documents.json");

        _documentStorage = dataLoader.LoadAllDocuments().ToDictionary(doc => doc.Id, doc => doc);
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
    /// Add or update a document and persist to disk
    /// </summary>
    public async Task SaveDocumentAsync(DocumentModel doc)
    {
        _documentStorage[doc.Id] = doc;

        // Write to disk
        Directory.CreateDirectory(Path.GetDirectoryName(_docPath)!);
        var allDocs = _documentStorage.Values.ToList();
        var json = JsonSerializer.Serialize(allDocs, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_docPath, json);
    }

    /// <summary>
    /// Check if a document exists
    /// </summary>
    public bool ContainsDocument(string documentId) => _documentStorage.ContainsKey(documentId);
}
