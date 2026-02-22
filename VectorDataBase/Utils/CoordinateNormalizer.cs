using System;
using System.Collections.Generic;
using System.Linq;
using VectorDataBase.Models;

namespace VectorDataBase.Utils;

/// <summary>
/// Handles 3D coordinate normalization with z-score standardization
/// </summary>
public sealed class CoordinateNormalizer
{
    public record NormalizationParams(
        float MeanX, float MeanY, float MeanZ,
        float StdX, float StdY, float StdZ);

    /// <summary>
    /// Normalize a list of 3D coordinates and return normalization parameters
    /// </summary>
    public (List<(float x, float y, float z)> normalized, NormalizationParams parameters)
        Normalize3D(List<List<float>> coords)
    {
        var points = coords.Select(c =>
        {
            var x = c.Count > 0 ? c[0] : 0f;
            var y = c.Count > 1 ? c[1] : 0f;
            var z = c.Count > 2 ? c[2] : 0f;
            return (x, y, z);
        }).ToList();

        if (points.Count == 0)
        {
            return (points, new NormalizationParams(0, 0, 0, 1, 1, 1));
        }

        var meanX = points.Average(p => p.x);
        var meanY = points.Average(p => p.y);
        var meanZ = points.Average(p => p.z);

        var stdX = (float)Math.Sqrt(points.Average(p => Math.Pow(p.x - meanX, 2)));
        var stdY = (float)Math.Sqrt(points.Average(p => Math.Pow(p.y - meanY, 2)));
        var stdZ = (float)Math.Sqrt(points.Average(p => Math.Pow(p.z - meanZ, 2)));

        stdX = stdX == 0 ? 1 : stdX;
        stdY = stdY == 0 ? 1 : stdY;
        stdZ = stdZ == 0 ? 1 : stdZ;

        var normParams = new NormalizationParams(meanX, meanY, meanZ, stdX, stdY, stdZ);

        var normalized = points.Select(p => (
            (float)((p.x - meanX) / stdX),
            (float)((p.y - meanY) / stdY),
            (float)((p.z - meanZ) / stdZ)
        )).ToList();

        return (normalized, normParams);
    }

    /// <summary>
    /// Normalize PCA nodes in-place and return normalization parameters
    /// </summary>
    public NormalizationParams NormalizePcaNodes(Dictionary<int, PCANode> nodes)
    {
        if (nodes.Count == 0)
        {
            return new NormalizationParams(0, 0, 0, 1, 1, 1);
        }

        var xs = nodes.Values.Select(n => n.ReducedVector.Length > 0 ? n.ReducedVector[0] : 0f).ToList();
        var ys = nodes.Values.Select(n => n.ReducedVector.Length > 1 ? n.ReducedVector[1] : 0f).ToList();
        var zs = nodes.Values.Select(n => n.ReducedVector.Length > 2 ? n.ReducedVector[2] : 0f).ToList();

        var meanX = xs.Average();
        var meanY = ys.Average();
        var meanZ = zs.Average();

        var stdX = (float)Math.Sqrt(xs.Average(v => Math.Pow(v - meanX, 2)));
        var stdY = (float)Math.Sqrt(ys.Average(v => Math.Pow(v - meanY, 2)));
        var stdZ = (float)Math.Sqrt(zs.Average(v => Math.Pow(v - meanZ, 2)));

        stdX = stdX == 0 ? 1 : stdX;
        stdY = stdY == 0 ? 1 : stdY;
        stdZ = stdZ == 0 ? 1 : stdZ;

        var normParams = new NormalizationParams(meanX, meanY, meanZ, stdX, stdY, stdZ);

        foreach (var n in nodes.Values)
        {
            var vec = n.ReducedVector;
            if (vec.Length < 3) Array.Resize(ref vec, 3);

            vec[0] = (float)((vec[0] - meanX) / stdX);
            vec[1] = (float)((vec[1] - meanY) / stdY);
            vec[2] = (float)((vec[2] - meanZ) / stdZ);

            n.ReducedVector = vec;
        }

        return normParams;
    }

    /// <summary>
    /// Apply normalization parameters to a single 3D point
    /// </summary>
    public float[] ApplyNormalization(float[] point, NormalizationParams parameters)
    {
        if (point == null || point.Length < 3) return [0, 0, 0];

        // Use a small epsilon to prevent division by zero or handle 0 std dev
        float safeStdX = parameters.StdX == 0 ? 1f : parameters.StdX;
        float safeStdY = parameters.StdY == 0 ? 1f : parameters.StdY;
        float safeStdZ = parameters.StdZ == 0 ? 1f : parameters.StdZ;
        
        return
        [
            (point[0] - parameters.MeanX) / parameters.StdX,
            (point[1] - parameters.MeanY) / parameters.StdY,
            (point[2] - parameters.MeanZ) / parameters.StdZ
        ];
    }
}
