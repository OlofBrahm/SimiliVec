using System;
using System.Collections.Generic;
using VectorDataBase.Core;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Linq;

namespace VectorDataBase.PCA;
public class PCAConversion
{
    private readonly MLContext _mlContext = new MLContext();
    private ITransformer? _pcaModel;

    //Convert HNSW nodes to PCA reduced nodes
    public Dictionary<int, PCANode> ConvertToPCA(Dictionary<int, HnswNode> nodes, int outputDimensions = 3)
    {
        var data = nodes.Values.Select(n => new VectorData { Features = n.Vector}).ToList();
        var trainData = _mlContext.Data.LoadFromEnumerable(data);

        var pcaPipeline = _mlContext.Transforms.ProjectToPrincipalComponents(
            outputColumnName: "PCAFeatures",
            inputColumnName: "Features",
            rank: outputDimensions);

        _pcaModel = pcaPipeline.Fit(trainData);

        var transformedData = _pcaModel.Transform(trainData);
        var pcaResults = _mlContext.Data.CreateEnumerable<PCAResult>(transformedData, reuseRowObject: false).ToList();

        var resultDict = new Dictionary<int, PCANode>();
        var nodeList = nodes.Values.ToList();
        
        for (int i = 0; i < nodeList.Count; i++)
        {
            resultDict.Add(nodeList[i].id, new PCANode
            {
                Id = nodeList[i].id,
                ReducedVector = ApplyPCA(pcaResults[i].PCAFeatures)
            });
        }
        return resultDict;
    }
    public float[] Transform(float[] vector)
    {
        if(_pcaModel == null)throw new InvalidOperationException("PCA model must be trained first");
        var engine = _mlContext.Model.CreatePredictionEngine<VectorData, PCAResult>(_pcaModel);
        var prediction = engine.Predict(new VectorData {Features = vector});
        return prediction.PCAFeatures;
    }

    private float[] ApplyPCA(float[] vector)
    {
        // Placeholder for PCA logic
        return vector;
    }
    private class VectorData
    {
        [VectorType(384)]
        public float[] Features { get; set; } = Array.Empty<float>();
    }

    private class PCAResult
    {
        [VectorType]
        public float[] PCAFeatures { get; set; } = Array.Empty<float>();
    }
}

public class PCANode
{
    public int Id {get; set;}
    public float[] ReducedVector {get; set;} = Array.Empty<float>();
}
