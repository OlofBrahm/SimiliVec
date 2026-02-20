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
        // Core/index - register configured DataIndex instance
        services.AddSingleton<IDataIndex>(sp => new DataIndex
        {
            MaxNeighbours = 4,
            EfConstruction = 20,
            InverseLogM = 1.0f / 1.5f
        });

        // Embedding and tokenizer
        services.AddSingleton<IEmbeddingModel, EmbeddingModel>();

        // Data loading
        services.AddSingleton<IDataLoader, DataLoader>();

        // PCA and UMAP conversion
        services.AddSingleton<PCAConversion>();
        services.AddSingleton<UmapConversion>();

        // service layer components
        services.AddSingleton<DocumentRepository>();
        services.AddSingleton<NodeDocumentMapper>();
        services.AddSingleton<CoordinateNormalizer>();

        // VectorService holds state and coordinates index/embeddings
        services.AddSingleton<IVectorService, VectorService>();


        return services;
    }
}
