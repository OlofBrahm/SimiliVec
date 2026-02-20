using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.VisualBasic;
using System.Text.Json;

namespace VectorDataBase.DimensionalityReduction.UMAP;

public class UmapConversion
{
    private readonly HttpClient _httpClient;
    private const string umapUrl = "http://127.0.0.1:8000/project-knn";
    private const string fitUrl = "http://127.0.0.1:8000/fit-vectors";
    private const string transformUrl = "http://127.0.0.1:8000/transform-vector";

    public UmapConversion()
    {
        _httpClient = new HttpClient();
    }

    private class UmapRequest
    {
        [JsonPropertyName("indices")]
        public int[][]? Indices { get; set; }
        [JsonPropertyName("distances")]
        public float[][]? Distances { get; set; }
        [JsonPropertyName("n_epochs")]
        public int NEpochs { get; set; }
    }

    private class FitRequest
    {
        [JsonPropertyName("vectors")]
        public float[][]? Vectors { get; set; }
        [JsonPropertyName("n_neighbors")]
        public int NNeighbors { get; set; } = 15;
        [JsonPropertyName("n_epochs")]
        public int NEpochs { get; set; } = 200;
        [JsonPropertyName("n_components")]
        public int NComponents { get; set; } = 3;
        [JsonPropertyName("random_state")]
        public int RandomState { get; set; } = 42;
    }

    private class TransformRequest
    {
        [JsonPropertyName("vector")]
        public float[]? Vector { get; set; }
    }

    private class UmapResponse
    {
        [JsonPropertyName("coordinates")]
        public List<List<float>>? Coordinates { get; set; }
    }

    private class TransformResponse
    {
        [JsonPropertyName("coordinates")]
        public List<float>? Coordinates { get; set; }
    }

    public async Task<List<List<float>>> GetUmapProjectionAsync(int[][] indices, float[][] distances, int epochs = 200)
    {
        var payload = new UmapRequest
        {
            Indices = indices,
            Distances = distances,
            NEpochs = epochs
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(umapUrl, payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Python Error: {error}");
                throw new Exception($"Python API Error: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<UmapResponse>();
            return result?.Coordinates ?? new List<List<float>>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to UMAP service: {ex.Message}");
            throw;
        }
    }

    public async Task<List<List<float>>> FitAndProjectAsync(float[][] vectors)
    {
        var req = new FitRequest { Vectors = vectors };
        var res = await _httpClient.PostAsJsonAsync(fitUrl, req);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<UmapResponse>();
        return body?.Coordinates ?? new List<List<float>>();
    }

    public async Task<float[]> TransformQueryAsync(float[] vector)
    {
        var req = new TransformRequest { Vector = vector };
        var res = await _httpClient.PostAsJsonAsync(transformUrl, req);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<TransformResponse>();
        return body?.Coordinates?.ToArray() ?? new float[3];
    }
}
