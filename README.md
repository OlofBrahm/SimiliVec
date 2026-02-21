# SimiliVec: Interactive Vector Engine & Visualizer

## Project Overwiew
**SimiliVec** is a full-stack vector database and visualization platform designed to explore high-performance similarity search and high-dimensional data analysis. The project moves beyond simple storage by providing a complete pipeline—from raw text embedding to interactive 3D visualization—allowing users to "see" how different mathematical approaches organize data.

---
### Core Components

* **Native Vector Engine (C#):** A high-performance implementation of the **Hierarchical Navigable Small World (HNSW)** algorithm for efficient Approximate Nearest Neighbor (ANN) search.
* **Neural Embeddings:** Integrated support for the **E5 Transformer model**, transforming text into high-quality vector representations directly within the .NET ecosystem using ONNX Runtime.
* **Hybrid Dimension Reduction:**
    * **PCA (Linear):** Handled natively in C# using the **Microsoft.ML** library for fast, deterministic linear projection.
    * **UMAP (Non-Linear):** Managed by a dedicated **Python-based microservice** to leverage the `umap-learn` ecosystem for complex manifold approximation.
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
5. **Start the UMAP-service (python):**
   ```bash
   cd services/umap-services
   pip install -r requirements.txt
   python main.py
   ```
---
## Project Structure
* **SimiliVec.Api:** The C# ASP.NET Core backend and entry point.
* **ReactFront/SimiliVecReact:** Modern React frontend for vector visualization.
* **VectorDataBase:** Core logic for the custom vector database, E5 embedding model and PCA conversion.
* **Umap-services:** Python-based Flask/FastAPI service for UMAP reduction.

---
## License & Credits
This project is MIT licensed. 
Third-party components (UMAP and E5-small-v2) are used under their respective licenses. 
See [CREDITS.md](./CREDITS.md) for full details and citations.
