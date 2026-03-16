using System;
using System.Collections.Generic;
using UMAPuwotSharp;
using System.Linq;
using System.Threading.Tasks;
using VectorDataBase.Core;
using System.Threading;
using System.Reflection.Metadata.Ecma335;

namespace VectorDataBase.DimensionalityReduction.UMAP;

public class UmapConversion
{
    private UMapModel? _trainedModel;
    private Dictionary<int, float[]>? _trainingVectors; // Store training data for k-NN lookup
    private List<List<float>>? _training3DCoords; // Store 3D coordinates

    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    public async Task<List<List<float>>> GetUmapProjectionAsync(Dictionary<int, HnswNode> nodes)
    {
        if (nodes == null || nodes.Count == 0) return new List<List<float>>();

        // Order by id so the indices match the output embedding exactly
        var nodesList = nodes.Values.OrderBy(n => n.Id).ToList();
        int rowCount = nodesList.Count;
        int dimCount = nodesList[0].Vector.Length;

        //The UMAP model expects a multidemensional array
        float[,] trainingData = new float[rowCount, dimCount];

        //fill the grid
        for (int i = 0; i < rowCount; i++)
        {
            float[] nodeVec = nodesList[i].Vector;

            // Calculate magnitude for normalization
            double sumSq = 0;
            for (int j = 0; j < dimCount; j++) sumSq += nodeVec[j] * nodeVec[j];
            float magnitude = (float)Math.Sqrt(sumSq);

            for (int j = 0; j < dimCount; j++)
            {
                // If magnitude is 0, just leave as 0, otherwise normalize
                trainingData[i, j] = magnitude > 0 ? nodeVec[j] / magnitude : 0;
            }
        }

        await _lock.WaitAsync();
        try
        {
            return await Task.Run(() =>
            {
                if (_trainingVectors == null || _trainingVectors.Count == 0)
                {
                    // Store training data for k-NN interpolation (workaround for broken Transform)
                    _trainingVectors = new Dictionary<int, float[]>();
                }

                for (int i = 0; i < rowCount; i++)
                {
                    var vec = new float[dimCount];
                    for (int j = 0; j < dimCount; j++)
                    {
                        vec[j] = trainingData[i, j];
                    }
                    _trainingVectors[i] = vec;
                }

                _trainedModel = new UMapModel();
                
                float[,] embedding = _trainedModel.FitWithProgress(
                    data: trainingData,
                    progressCallback: (phase, current, total, percent, message) =>
                    {
                        if (percent % 10 == 0) Console.WriteLine($"[{phase}] {percent:F1} - {message}");
                    },
                    embeddingDimension: 3,
                    metric: DistanceMetric.Cosine
                // Using HNSW (forceExactKnn=false by default)
                );

                // Convert output back to List<list<float>> for the api
                var result = new List<List<float>>();
                for (int i = 0; i < embedding.GetLength(0); i++)
                {
                    result.Add(new List<float> { embedding[i, 0], embedding[i, 1], embedding[i, 2] });
                }

                // Store 3D coordinates for k-NN interpolation
                _training3DCoords = result;

                // Debug: Check training coordinate ranges
                if (result.Count > 0)
                {
                    var minX = result.Min(r => r[0]);
                    var maxX = result.Max(r => r[0]);
                    var minY = result.Min(r => r[1]);
                    var maxY = result.Max(r => r[1]);
                    var minZ = result.Min(r => r[2]);
                    var maxZ = result.Max(r => r[2]);
                    Console.WriteLine($"[UMAP Training Coords] X:[{minX:F2}, {maxX:F2}], Y:[{minY:F2}, {maxY:F2}], Z:[{minZ:F2}, {maxZ:F2}]");
                }

                return result;
            });
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<List<float>>> GetUmapNodesAsync()
    {
        if(_training3DCoords != null)
        {
            return _training3DCoords;
        }
        throw new InvalidOperationException("Need to run GetUmapProjection first");
    }
    public async Task<float[]> TransformQueryAsync(float[] vector)
    {
        await _lock.WaitAsync();
        try
        {
            if (_trainingVectors == null || _training3DCoords == null)
            {
                throw new InvalidOperationException("Must be trained with GetUmapProjectionAsync before transforming queries");
            }

            // Normalize the query vector
            double sumSq = 0;
            for (int i = 0; i < vector.Length; i++) sumSq += vector[i] * vector[i];
            float magnitude = (float)Math.Sqrt(sumSq);

            float[] normalizedQuery = new float[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                normalizedQuery[i] = magnitude > 0 ? vector[i] / magnitude : 0;
            }

            return await Task.Run(() =>
            {
                // Workaround for broken Transform: Use k-NN interpolation
                // Find k nearest neighbors in high-dimensional space
                int k = Math.Min(10, _trainingVectors.Count);
                var distances = new List<(int index, float distance)>();

                foreach (var kvp in _trainingVectors)
                {
                    float cosineDist = ComputeCosineDistance(normalizedQuery, kvp.Value);
                    distances.Add((kvp.Key, cosineDist));
                }

                var nearestNeighbors = distances.OrderBy(d => d.distance).Take(k).ToList();

                Console.WriteLine($"[UMAP k-NN Interpolation] Using {k} nearest neighbors, closest distance: {nearestNeighbors[0].distance:F4}");

                // Weighted average of neighbors' 3D positions (inverse distance weighting)
                float totalWeight = 0;
                float x = 0, y = 0, z = 0;

                foreach (var (index, dist) in nearestNeighbors)
                {
                    // Use inverse distance as weight (add small epsilon to avoid division by zero)
                    float weight = 1.0f / (dist + 0.0001f);
                    totalWeight += weight;

                    var coord = _training3DCoords[index];
                    x += coord[0] * weight;
                    y += coord[1] * weight;
                    z += coord[2] * weight;
                }

                var result = new float[]
                {
                    x / totalWeight,
                    y / totalWeight,
                    z / totalWeight
                };

                Console.WriteLine($"[UMAP Interpolated Output] X:{result[0]:F2}, Y:{result[1]:F2}, Z:{result[2]:F2}");
                return result;
            });
        }
        finally
        {
            _lock.Release();
        }
    }

    private float ComputeCosineDistance(float[] a, float[] b)
    {
        float dotProduct = 0;
        float normA = 0;
        float normB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        float cosineSimilarity = dotProduct / (float)(Math.Sqrt(normA) * Math.Sqrt(normB) + 1e-10);
        return 1.0f - cosineSimilarity; // Convert similarity to distance
    }
}
