using Microsoft.AspNetCore.Mvc;
using VectorDataBase.Datahandling;

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
            var nodes = await _vectorService.GetUmapNodes();
            if(nodes == null || nodes.Count == 0)
            {
                return Ok(new List<object>()); //Returns a empty list instead of throwing error
            }
            return Ok(nodes);
        }
        catch(InvalidOperationException ex)
        {
            return BadRequest($"Cannot perfom PCA: {ex.Message}");
        }
        catch (Exception ex)
        {
            return StatusCode(503, $"UMAP Service unavailible: {ex.Message}");
        }
    }
    [HttpPost("documents")]
    public async Task<IActionResult> AddDocument([FromBody] DocumentModel input)
    {
        if (input == null || string.IsNullOrWhiteSpace(input.Id) || string.IsNullOrWhiteSpace(input.Content))
            return BadRequest("Id and Content are required.");

        var doc = new DocumentModel
        {
            Id = input.Id.Trim(),
            Content = input.Content,
            MetaData = input.MetaData ?? new Dictionary<string, string>()
        };

        await _vectorService.AddDocument(doc, indexChunks: true);
        await GetUmapNodes();
        return Ok(new { id = doc.Id });
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