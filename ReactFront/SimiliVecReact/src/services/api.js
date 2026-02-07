import axios from 'axios';

const API_URL = "http://localhost:5202/api/vector"

export const vectorApi = {
    getNodes: async () => {
        const response = await axios.get(`${API_URL}/nodes`);
        return response.data;
    },

    search: async (queryText) => {
        const respone = await axios.post(`${API_URL}/search?k=5`, JSON.stringify(queryText), {
            headers: { 'Content-Type': 'application/json' }
        });
        return respone.data;
    },

    addDocument: async (doc) => {
        const response = await axios.post(`${API_URL}/documents', doc`);
        return response.data;
    }
}
