using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VectorDataBase.DimensionalityReduction.UMAP;

public class UmapConversion
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public UmapConversion()
    {
        _httpClient = new HttpClient();
        _baseUrl = Environment.GetEnvironmentVariable("UMAP_SERVICE_URL") 
                   ?? "http://127.0.0.1:8000";
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
        public int NNeighbors { get; set; } = 30;
        [JsonPropertyName("n_epochs")]
        public int NEpochs { get; set; } = 50;
        [JsonPropertyName("n_components")]
        public int NComponents { get; set; } = 3;
        [JsonPropertyName("random_state")]
        public int RandomState { get; set; } = 42;
        [JsonPropertyName("min_dist")]
        public float MinDist { get; set; } = 0.01f;
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
            Console.WriteLine("Calling UMAP API");
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/project-knn", payload);

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
        var res = await _httpClient.PostAsJsonAsync($"{_baseUrl}/fit-vectors", req);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<UmapResponse>();
        return body?.Coordinates ?? new List<List<float>>();
    }

    public async Task<float[]> TransformQueryAsync(float[] vector)
    {
        var req = new TransformRequest { Vector = vector };
        var res = await _httpClient.PostAsJsonAsync($"{_baseUrl}/transform-vector", req);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<TransformResponse>();
        return body?.Coordinates?.ToArray() ?? new float[3];
    }
}
