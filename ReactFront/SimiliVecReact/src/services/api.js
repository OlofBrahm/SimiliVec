import axios from 'axios';

// Use environment variable, fallback to localhost for development
const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5202";
const API_BASE = `${API_URL}/api/vector`;

export const vectorApi = {
    getPCANodes: async () => {
        const response = await axios.get(`${API_BASE}/nodes/pca`);
        return response.data;
    },

    getUMAPNodes: async () => {
        const response = await axios.get(`${API_BASE}/nodes/umap`);
        return response.data;
    },

    search: async (queryText) => {
        const response = await axios.post(`${API_BASE}/search?k=5`, JSON.stringify(queryText), {
            headers: { 'Content-Type': 'application/json' }
        });
        return response.data;
    },

    searchUmap: async (queryText) => {
        const response = await axios.post(`${API_BASE}/search/umap?k=5`, JSON.stringify(queryText), {
            headers: { 'Content-Type': 'application/json' }
        });
        return response.data;
    },

    addDocument: async (doc) => {
        const response = await axios.post(`${API_BASE}/documents`, doc);
        return response.data;
    }
}
