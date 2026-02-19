from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import umap
import numpy as np
from typing import List

app = FastAPI(title="SimiliVec UMAP Converter")

_reducer = None  # stored fitted reducer

class FitRequest(BaseModel):
    vectors: List[List[float]]
    n_neighbors: int = 15
    n_epochs: int = 200
    n_components: int = 3
    random_state: int = 42

class TransformRequest(BaseModel):
    vector: List[float]

@app.post('/fit-vectors')
async def fit_vectors(data: FitRequest):
    global _reducer
    try:
        X = np.array(data.vectors, dtype=np.float32)

        _reducer = umap.UMAP(
            n_neighbors=data.n_neighbors,
            n_epochs=data.n_epochs,
            n_components=data.n_components,
            random_state=data.random_state,
            init='spectral'
        )

        embedding = _reducer.fit_transform(X)
        return {"coordinates": embedding.tolist()}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post('/transform-vector')
async def transform_vector(data: TransformRequest):
    try:
        if _reducer is None:
            raise HTTPException(status_code=400, detail="UMAP reducer not fitted yet.")
        x = np.array([data.vector], dtype=np.float32)
        coords = _reducer.transform(x)
        return {"coordinates": coords[0].tolist()}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
