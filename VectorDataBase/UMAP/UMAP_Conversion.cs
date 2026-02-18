using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VectorDataBase.UMAP
{
    public class UmapConversion{
        private readonly HttpClient _httpClient;
        private const string umapUrl = "http://127.0.0.1:8000/project-knn";

        public UmapConversion()
        {
            _httpClient = new HttpClient();
        }

        private class UmapRequest
        {
            [JsonPropertyName("indices")]
            public int[][]? Indices {get; set;}
            [JsonPropertyName("distances")]
            public float[][]? Distances {get; set;}
            [JsonPropertyName("n_epochs")]
            public int NEpochs {get; set;}
        }

        private class UmapResponse
        {
            [JsonPropertyName("coordinates")]
            public List<List<float>>? Coordinates {get; set;}
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
                    throw new Exception($"Python API Error: {error}");
                }

                var result = await response.Content.ReadFromJsonAsync<UmapResponse>();
                return result?.Coordinates ?? new List<List<float>>();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to connect to UMAP service: {ex.Message}");
                throw;
            }
        }
    }
}