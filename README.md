# SimiliVec: Interactive Vector Engine & Visualizer

## Project Overwiew
**SimiliVec** is a full-stack vector database and visualization platform designed to explore high-performance similarity search and high-dimensional data analysis. The project moves beyond simple storage by providing a complete pipeline, from raw text embedding to interactive 3D visualization, allowing users to "see" how different mathematical approaches organize data.

---
### Core Components

* **Native Vector Engine (C#):** A high-performance implementation of the **Hierarchical Navigable Small World (HNSW)** algorithm for efficient Approximate Nearest Neighbor (ANN) search.
* **Neural Embeddings:** Integrated support for the **E5 Transformer model**, transforming text into high-quality vector representations directly within the .NET ecosystem using ONNX Runtime.
* **Hybrid Dimension Reduction:**
    * **PCA (Linear):** Handled natively in C# using the **Microsoft.ML** library for fast, deterministic linear projection.
    * **UMAP (Non-Linear):** Implemented natively in C# via UMAPuwotSharp, removing the need for external Python dependencies while maintaining complex manifold approximation.
* **Interactive Dashboard (React):** A modern frontend built to visualize vector clusters in 3D space, enabling users to interact with data and validate search results visually.

---

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Node.js & npm (for the React frontend)
- PowerShell (Windows) or Bash (Linux/Mac)

### Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone https://github.com/OlofBrahm/SimiliVec
   cd SimiliVec
   ```

2. **Download the E5 model and tokenizer files:**

   **On Windows (PowerShell):**
   ```powershell
   .\setup-models.ps1
   ```

   **On Linux/Mac (Bash):**
   ```bash
   chmod +x setup-models.sh
   ./setup-models.sh
   ```

   This script will download:
   - E5-Small-V2 model (133 MB)
   - Tokenizer files (vocab.txt, tokenizer.json, tokenizer_config.json)

3. **Build and run the backend:**
   ```bash
   cd SimiliVec.Api
   dotnet run
   ```

4. **Run the frontend(react):**
   ```bash
   cd ReactFront/SimiliVecReact
   npm install
   npm run dev
   ```

---

## Project Structure

* **SimiliVec.Api:** The ASP.NET Core 9.0 entry point, providing high-performance REST endpoints for vector management, semantic search, and visualization data.
* **ReactFront/SimiliVecReact:** A high-performance 3D visualization dashboard built with React and Three.js to render and navigate large-scale vector clusters.
* **VectorDataBase (Core Library):** The engine room of the project, containing:
    * **HNSW Indexing:** A native C# implementation of Hierarchical Navigable Small Worlds for efficient $O(\log N)$ similarity search.
    * **E5 Embeddings:** Local transformer inference using **ONNX Runtime**, eliminating the need for external LLM API costs.
    * **Native Dimensionality Reduction:**
        * **PCA:** Fast, linear reduction for global data structure preservation.
        * **UMAP:** Non-linear manifold learning via native C# integration for complex cluster discovery.



---

## License & Credits

This project is released under the **MIT License**. See the [LICENSE](./LICENSE) file for the full text.

**Third-Party Credits:**
* **UMAPuwotSharp:** Native .NET port of the UMAP algorithm.
* **E5-small-v2:** Text embedding model provided by Microsoft/intfloat.
* **ML.NET:** Powering the native PCA implementation.

For detailed academic citations and third-party license notices, please refer to [CREDITS.md](./CREDITS.md).
