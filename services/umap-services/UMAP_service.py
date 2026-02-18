from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import umap
import numpy as np
from typing import List

app = FastAPI(title="SimiliVec UMAP Converter")

# Data structure to match the call from C#
class KnnSnapshot(BaseModel):
    indicies: List[List[int]]
    distances: List[List[float]]
    n_epochs: int = 200 #Amount of iterations for the optimization

@app.post ('/project-knn')
async def project_knn(data: KnnSnapshot):
    try:
        knn_indices = np.array(data.indices)
        knn_distances = np.array(data.distances)
        
        #Setup UMAP, with precomputed metric
        reducer = umap.UMAP(
            metric='precomputed',
            n_neighbors=knn_indices.shape[1],
            n_epochs=data.n_epochs,
            init='spectral'
        )

        X_dummy = np.zeros((knn_indices.shape[0], 1))
        reducer._knn_indices = knn_indices
        reducer._knn_dists = knn_distances

        embedding = reducer.fit_transform(X_dummy)

        return {"coordinates": embedding.tolist()}
    
    except Exception as e:
        print(f"Error: {e}")
        raise HTTPException(status_code=500, detail=str(e))
    
if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8000)
