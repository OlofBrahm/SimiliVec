using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VectorDataBase.Interfaces;
using VectorDataBase.Embedding;
using VectorDataBase.Core;
using VectorDataBase;
using VectorDataBase.DimensionalityReduction.PCA;
using VectorDataBase.DimensionalityReduction.UMAP;
using VectorDataBase.Repositories;
using VectorDataBase.Utils;

namespace VectorDataBase.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVectorDataBaseServices(this IServiceCollection services)
    {
        return services.AddVectorDataBaseProductionServices();
    }

    public static IServiceCollection AddVectorDataBaseDemoServices(this IServiceCollection services)
    {
        AddSharedServices(services);

        services.AddSingleton(new DocumentStoreOptions { PreferSampleData = true });
        services.AddSingleton<IDocumentStore, DataLoader>();
        // DocumentRepository is initialized via InitializeDocumentRepositoryAsync after app.Build()
        services.AddSingleton<IDocumentRepository>(sp => GetInitializedRepository());

        services.AddSingleton<IVectorService, VectorService>();

        return services;
    }

    public static IServiceCollection AddVectorDataBaseProductionServices(this IServiceCollection services)
    {
        AddSharedServices(services);

        services.AddSingleton(new DocumentStoreOptions { PreferSampleData = false });
        services.AddSingleton<IDocumentStore, DataLoader>();
        // DocumentRepository is initialized via InitializeDocumentRepositoryAsync after app.Build()
        services.AddSingleton<IDocumentRepository>(sp => GetInitializedRepository());

        services.AddSingleton<IVectorService, VectorService>();

        return services;
    }

    private static void AddSharedServices(IServiceCollection services)
    {
        // Core/index - register configured DataIndex instance
        services.AddSingleton<IDataIndex>(sp => new DataIndex
        {
            MaxNeighbours = 16,
            EfConstruction = 100,
            InverseLogM = 1.0f / 1.5f
        });

        // Embedding and tokenizer
        services.AddSingleton<IEmbeddingModel, EmbeddingModel>();

        // PCA and UMAP conversion
        services.AddSingleton<PCAConversion>();
        services.AddSingleton<UmapConversion>();

        // service layer components
        services.AddSingleton<NodeDocumentMapper>();
        services.AddSingleton<CoordinateNormalizer>();
    }

    // Static field to store the initialized repository - workaround for post-Build() initialization
    private static DocumentRepository? _initializedRepository = null;

    /// <summary>
    /// Asynchronously initializes the DocumentRepository and stores it for later retrieval.
    /// Must be called after app.Build() and before using VectorService.
    /// </summary>
    public static async Task InitializeDocumentRepositoryAsync(this IServiceProvider serviceProvider)
    {
        var documentStore = serviceProvider.GetRequiredService<IDocumentStore>();
        _initializedRepository = await DocumentRepository.CreateAsync(documentStore);
    }

    /// <summary>
    /// Gets the initialized DocumentRepository. Throws if not yet initialized.
    /// </summary>
    public static DocumentRepository GetInitializedRepository()
    {
        if (_initializedRepository is null)
        {
            throw new InvalidOperationException(
                "DocumentRepository has not been initialized. " +
                "Ensure InitializeDocumentRepositoryAsync is called after app.Build().");
        }
        return _initializedRepository;
    }
}
