import axios from 'axios';

const API_URL = "http://localhost:5202/api/vector"

export const vectorApi = {
    getPCANodes: async () => {
        const response = await axios.get(`${API_URL}/nodes/pca`);
        return response.data;
    },

    getUMAPNodes: async () => {
        const response = await axios.get(`${API_URL}/nodes/umap`);
        return response.data;
    },

    search: async (queryText) => {
        const response = await axios.post(`${API_URL}/search?k=5`, JSON.stringify(queryText), {
            headers: { 'Content-Type': 'application/json' }
        });
        return response.data;
    },

    searchUmap: async (queryText) => {
        const response = await axios.post(`${API_URL}/search/umap?k=5`, JSON.stringify(queryText), {
            headers: { 'Content-Type': 'application/json' }
        });
        return response.data;
    },

    addDocument: async (doc) => {
        const response = await axios.post(`${API_URL}/documents`, doc);
        return response.data;
    }
}
