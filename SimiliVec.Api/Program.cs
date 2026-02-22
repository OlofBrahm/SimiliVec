using System.Runtime.InteropServices;
using VectorDataBase.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Railway port support - only override if PORT env var is explicitly set
var portEnvVar = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(portEnvVar))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{portEnvVar}");
}

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins"; 

// Get frontend URL from environment variable for production CORS
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          if (!string.IsNullOrEmpty(frontendUrl))
                          {
                              // Production: specific origin
                              policy.WithOrigins(frontendUrl)
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
                          }
                          else
                          {
                              // Development: allow any origin
                              policy.AllowAnyOrigin() 
                                    .AllowAnyHeader()
                                    .AllowAnyMethod();
                          }
                      });
});

builder.Services.AddVectorDataBaseServices();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Allow NaN and Infinity values in JSON responses
        options.JsonSerializerOptions.NumberHandling = 
            System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Initialize and index documents on startup BEFORE starting the app
Console.WriteLine("Pre-startup: About to index documents...");
try
{
    var vectorService = app.Services.GetRequiredService<IVectorService>();
    Console.WriteLine("Pre-startup: Got vector service");
    var indexTask = vectorService.IndexDocument();
    indexTask.Wait(); // Block and wait for indexing to complete
    Console.WriteLine("Pre-startup: Documents indexed on startup successfully.");
    
    Console.WriteLine("Pre-startup: Training PCA model");
    var initialPCANodes = await vectorService.GetPCANodes();
    Console.WriteLine($"Pre-startup: PCA Model trained on {initialPCANodes.Count}");
    
    // ADD THIS: Pre-train UMAP model on startup
    Console.WriteLine("Pre-startup: Training UMAP model...");
    
    // Check if native library exists
    var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    var libPath = isLinux ? "libuwot.so" : "uwot.dll";
    var searchDirs = new[] { 
        "/app",
        "/app/runtimes/linux-x64/native",
        AppContext.BaseDirectory,
        Path.Combine(AppContext.BaseDirectory, "runtimes/linux-x64/native")
    };
    
    bool foundLib = false;
    foreach (var dir in searchDirs)
    {
        var fullPath = Path.Combine(dir, libPath);
        if (File.Exists(fullPath))
        {
            Console.WriteLine($"✅ Found UMAP native library at: {fullPath}");
            foundLib = true;
            break;
        }
    }
    
    if (!foundLib)
    {
        Console.WriteLine($"❌ CRITICAL: {libPath} not found in any expected location!");
        Console.WriteLine("Searched directories:");
        foreach (var dir in searchDirs)
        {
            Console.WriteLine($"  - {dir}");
        }
        Console.WriteLine("UMAP will be unavailable.");
    }
    else
    {
        try 
        {
            var initialUMAPNodes = await vectorService.GetUmapNodes();
            Console.WriteLine($"✅ Pre-startup: UMAP Model trained on {initialUMAPNodes.Count} nodes");
        }
        catch (Exception umapEx)
        {
            Console.WriteLine($"❌ Pre-startup UMAP training failed: {umapEx.Message}");
            Console.WriteLine($"Stack trace: {umapEx.StackTrace}");
            if (umapEx.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {umapEx.InnerException.Message}");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Pre-startup error indexing documents: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
app.UseCors(MyAllowSpecificOrigins);
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();