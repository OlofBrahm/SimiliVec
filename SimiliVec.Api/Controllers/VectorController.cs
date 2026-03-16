using Microsoft.AspNetCore.Mvc;
using VectorDataBase.Models;

[ApiController]
[Route("api/[controller]")]
public class VectorController : ControllerBase
{
    private readonly IVectorService _vectorService;
    public VectorController(IVectorService vectorService)
    {
        _vectorService = vectorService;
    }

    [HttpPost("index")]
    public async Task<IActionResult> IndexDocument()
    {
        var documents = _vectorService.GetDocuments();
        if (documents == null || documents.Count == 0)
        {
            return BadRequest("No documents provided for indexing.");
        }

        await _vectorService.IndexDocument();
        return Ok("Documents indexed successfully");
    }

    [HttpPost("search")]
    public async Task<IActionResult> Search([FromBody] string query, [FromQuery] int k = 5)
    {
        if (string.IsNullOrEmpty(query))
        {
            return BadRequest("Search query can not be empty");
        }

        var results = await _vectorService.Search(query, k);
        return Ok(results);
    }
    [HttpGet("nodes/pca")]
    public async Task<IActionResult> GetPcaNodes()
    {
        try
        {
            var nodes = await _vectorService.GetPCANodes();
            return Ok(nodes);
        }
        catch(Exception ex)
        {
            return StatusCode(500, $"PCA ERROR: {ex.Message}");
        }

    }
    [HttpGet("nodes/umap")]
    public async Task<IActionResult> GetUmapNodes()
    {
        try
        {
            var totalNodes = _vectorService.GetDocuments().Count;
            Console.WriteLine($"[GetUmapNodes] Total documents: {totalNodes}");
            
            // UMAP needs minimum 3 nodes
            if (totalNodes < 3)
            {
                Console.WriteLine("[GetUmapNodes] Not enough nodes for UMAP, returning empty list");
                return Ok(new List<object>());
            }

            var nodes = await _vectorService.GetUmapCalculatedNodes();
            Console.WriteLine($"[GetUmapNodes] Successfully retrieved {nodes?.Count ?? 0} UMAP nodes");
            
            if(nodes == null || nodes.Count == 0)
            {
                return Ok(new List<object>());
            }
            return Ok(nodes);
        }
        catch(InvalidOperationException ex)
        {
            Console.WriteLine($"[GetUmapNodes] InvalidOperation: {ex.Message}");
            return BadRequest($"Cannot perform UMAP: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetUmapNodes] ERROR: {ex.Message}");
            Console.WriteLine($"[GetUmapNodes] Stack: {ex.StackTrace}");
            return StatusCode(503, new { error = $"UMAP Service unavailable: {ex.Message}", details = ex.StackTrace });
        }
    }
    [HttpPost("documents")]
    public async Task<IActionResult> AddDocument([FromBody] DocumentModel input)
    {
        try
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Id) || string.IsNullOrWhiteSpace(input.Content))
                return BadRequest("Id and Content are required.");

            var doc = new DocumentModel
            {
                Id = input.Id.Trim(),
                Content = input.Content,
                MetaData = input.MetaData ?? new Dictionary<string, string>()
            };

            Console.WriteLine($"[AddDocument] Starting to add document: {doc.Id}");
            await _vectorService.AddDocument(doc, indexChunks: true);
            Console.WriteLine($"[AddDocument] Successfully added document: {doc.Id}");
            return Ok(new { id = doc.Id, message = "Document added successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AddDocument] ERROR: {ex.Message}");
            Console.WriteLine($"[AddDocument] Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
    [HttpPost("search/umap")]
    public async Task<IActionResult> SearchUmap([FromBody] string query, [FromQuery] int k = 5)
    {
        if (string.IsNullOrEmpty(query))
            return BadRequest("Search query cannot be empty");

        var results = await _vectorService.SearchUmap(query, k);
        return Ok(results);
    }
}