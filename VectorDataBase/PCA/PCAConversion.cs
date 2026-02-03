using System;
using System.Collections.Generic;
using VectorDataBase.Core;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Linq;


namespace VectorDataBase.PCA;
public class PCAConversion
{
    private readonly MLContext _mlContext = new MLContext(seed: 42);
    private ITransformer? _pcaModel;


    // Embedding vector size
    private const int EmbeddingDim = 384;


    //Convert HNSW nodes to PCA reduced nodes
    public Dictionary<int, PCANode> ConvertToPCA(Dictionary<int, HnswNode> nodes, int outputDimensions = 3)
    {
        if (nodes == null) throw new ArgumentNullException(nameof(nodes));
        if (nodes.Count == 0) throw new InvalidOperationException("No vectors to train PCA.");
        
        var data = nodes.Values.Select(n => new VectorData { Id = n.id, Features = n.Vector }).ToList();

        if (data.Any(d => d.Features.Length != EmbeddingDim))
            throw new InvalidOperationException($"All vectors must have length {EmbeddingDim}.");


        outputDimensions = Math.Min(outputDimensions, EmbeddingDim);


        var trainData = _mlContext.Data.LoadFromEnumerable(data);


        var pcaPipeline = _mlContext.Transforms.ProjectToPrincipalComponents(
            outputColumnName: "PCAFeatures",
            inputColumnName: "Features",
            rank: outputDimensions,
            seed: 42);


        _pcaModel = pcaPipeline.Fit(trainData);


        var transformedData = _pcaModel.Transform(trainData);
        var pcaResults = _mlContext.Data.CreateEnumerable<PCAResult>(transformedData, reuseRowObject: false);


        var resultDict = new Dictionary<int, PCANode>(nodes.Count);
        foreach (var row in pcaResults)
        {
            resultDict[row.Id] = new PCANode
            {
                Id = row.Id,
                ReducedVector = row.PCAFeatures
            };
        }
        return resultDict;
    }


    public float[] Transform(float[] vector)
    {
        if (_pcaModel == null) throw new InvalidOperationException("PCA model must be trained first.");
        if (vector.Length != EmbeddingDim) throw new ArgumentException($"Expected vector length {EmbeddingDim}, got {vector.Length}.");


        var engine = _mlContext.Model.CreatePredictionEngine<VectorData, PCAResult>(_pcaModel);
        return engine.Predict(new VectorData { Features = vector }).PCAFeatures;
    }


    private class VectorData
    {
        public int Id { get; set; }
        [VectorType(EmbeddingDim)]
        public float[] Features { get; set; } = Array.Empty<float>();
    }


    private class PCAResult
    {
        public int Id { get; set; }
        [VectorType]
        public float[] PCAFeatures { get; set; } = Array.Empty<float>();
    }
}


public class PCANode
{
    public int Id { get; set; }
    public float[] ReducedVector { get; set; } = Array.Empty<float>();
}





