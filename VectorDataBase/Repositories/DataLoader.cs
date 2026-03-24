using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VectorDataBase.Interfaces;
using VectorDataBase.Models;

namespace VectorDataBase.Repositories;

/// <summary>
/// Handles loading and saving document data from persistent storage
/// </summary>
public class DataLoader : IDocumentStore
{
    private readonly string _dataDirectory;
    private readonly string _dataFileName;
    private readonly string _fullFilePath;
    private readonly string _sampleDataPath;
    private readonly bool _preferSampleData;

    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true
    };

    public DataLoader(DocumentStoreOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _dataFileName = "documents.json";
        _preferSampleData = options.PreferSampleData;
        
        // Try to find bundled sample data first (for production/demo)
        var appDirectory = AppContext.BaseDirectory;
        _sampleDataPath = Path.Combine(appDirectory, "SampleData", _dataFileName);
        
        // AppData folder for user-saved documents
        _dataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _dataDirectory = Path.Combine(_dataDirectory, "SimiliVec");
        _fullFilePath = Path.Combine(_dataDirectory, _dataFileName);

        if (!_preferSampleData)
        {
            try
            {
                if (!Directory.Exists(_dataDirectory))
                {
                    Directory.CreateDirectory(_dataDirectory);
                }

                // Initialize user data file if it doesn't exist
                if (!File.Exists(_fullFilePath))
                {
                    File.WriteAllText(_fullFilePath, "[]");
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or DirectoryNotFoundException)
            {
                Console.WriteLine($"DataLoader constructor: Failed to initialize data storage at {_fullFilePath}. Exception: {ex}");
                throw new InvalidOperationException("Failed to initialize document storage.", ex);
            }
        }
    }

    /// <summary>
    /// Ensures that the specified directory exists; if not, creates it.
    /// </summary>
    private static void EnsureDirectoryExists(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Loads documents from persistent storage asynchronously.
    /// </summary>
    public async Task<IEnumerable<DocumentModel>> LoadAllDocumentsAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => LoadAllDocumentsCore(), cancellationToken);
    }

    private IEnumerable<DocumentModel> LoadAllDocumentsCore()
    {
        if (_preferSampleData)
        {
            var sampleData = TryLoadDocumentsFromPath(_sampleDataPath);
            if (sampleData != null)
            {
                Console.WriteLine($"LoadAllDocumentsAsync: Demo mode loaded {sampleData.Count} documents from {_sampleDataPath}");
                return sampleData;
            }

            Console.WriteLine("LoadAllDocumentsAsync: Demo mode sample data missing, using empty list");
            return new List<DocumentModel>();
        }

        EnsureDirectoryExists(_fullFilePath);
        var primaryPath = _fullFilePath;
        var fallbackPath = _sampleDataPath;

        var primaryData = TryLoadDocumentsFromPath(primaryPath);
        if (primaryData != null)
        {
            Console.WriteLine($"LoadAllDocumentsAsync: Loaded {primaryData.Count} documents from {primaryPath}");
            return primaryData;
        }

        var fallbackData = TryLoadDocumentsFromPath(fallbackPath);
        if (fallbackData != null)
        {
            Console.WriteLine($"LoadAllDocumentsAsync: Loaded {fallbackData.Count} documents from {fallbackPath}");
            return fallbackData;
        }

        Console.WriteLine("LoadAllDocumentsAsync: No data found, using empty list");
        return new List<DocumentModel>();
    }

    private List<DocumentModel>? TryLoadDocumentsFromPath(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var jsonData = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<DocumentModel>>(jsonData, _jsonOptions) ?? new List<DocumentModel>();
        }
        catch (IOException ex)
        {
            Console.WriteLine($"TryLoadDocumentsFromPath: I/O error reading {path}: {ex.Message}");
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"TryLoadDocumentsFromPath: Access denied for {path}: {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"TryLoadDocumentsFromPath: Invalid JSON in {path}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TryLoadDocumentsFromPath: Unexpected error reading {path}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Saves all documents to persistent storage asynchronously.
    /// </summary>
    public async Task SaveAllDocumentsAsync(IEnumerable<DocumentModel> documents, CancellationToken cancellationToken = default)
    {
        if (documents is null)
        {
            throw new ArgumentNullException(nameof(documents), "Documents collection cannot be null.");
        }

        if (_preferSampleData)
        {
            Console.WriteLine("SaveAllDocumentsAsync: Demo mode is read-only; skipping persistence.");
            return;
        }

        EnsureDirectoryExists(_fullFilePath);
        try
        {
            var jsonData = JsonSerializer.Serialize(documents, _jsonOptions);
            await File.WriteAllTextAsync(_fullFilePath, jsonData, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SaveAllDocumentsAsync: Failed to save data to {_fullFilePath}. Exception: {ex}");
            throw;
        }
    }

}