# SimiliVec: Custom Vector Database

[![Status Badge](https://img.shields.io/badge/Status-In%20Development-blue)](https://github.com/your-username/SimiliVec) 
[![Language Badge](https://img.shields.io/badge/Built%20With-C%23-darkgreen)](https://docs.microsoft.com/en-us/dotnet/csharp/)

> ** STUDENT PROJECT WARNING ** > This project is a proof-of-concept and learning exercise. It is **NOT** production-ready and lacks critical features like persistence and concurrency.

---

## Project Overview

**SimiliVec** is a custom vector database designed to explore the mechanics of high-performance similarity search. The project focuses on a native C# implementation of modern vector database components:

* **Embedding Model:** Uses the **E5 transformer model** for generating high-quality vector representations.
* **Indexing Structure:** Implements the **Hierarchical Navigable Small World (HNSW)** graph for efficient approximate nearest neighbor search.
* **Node visualization** Implements PCA and UMAP dimension-reduction techniques to compare two different approaches.
---

## Key Features

*  High-performance **HNSW** index implementation.
*  Native integration with the **E5** embedding model.
*  Simple API for vector insertion and similarity search.
*  PCA and UMAP dimension reduction for visualization.

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

## Project Structure
* **SimiliVec.Api:** The C# ASP.NET Core backend and entry point.
* **ReactFront/SimiliVecReact:** Modern React frontend for vector visualization.
* **VectorDataBase: Core logic** for the custom vector database and E5 embedding model.
* **services/umap-services:** Python-based Flask/FastAPI service for UMAP and PCA reduction.


## License & Credits
This project is MIT licensed. 
Third-party components (UMAP and E5-small-v2) are used under their respective licenses. 
See [CREDITS.md](./CREDITS.md) for full details and citations.
