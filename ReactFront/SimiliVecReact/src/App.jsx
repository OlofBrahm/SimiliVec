import { useEffect, useState } from 'react'
import { Canvas } from '@react-three/fiber'
import { OrbitControls, Grid } from '@react-three/drei'
import { vectorApi } from './services/api'
import { Enviroment } from './components/Scene'
import { VectorNode } from './components/VectorNode'
import { QueryNode } from './components/QueryNode'
import { PopoutSearch } from './components/PopoutSearch'
import { PopoutMenu } from './components/PopoutMenu'
import { SearchResults } from './components/SearchResults'
import './App.css'


const API_URL = "http://localhost:5202/api/vector"

export default function App() {

  const [nodes, setNodes] = useState([]);
  const [mode, setMode] = useState('standard');
  const [selectedNode, setSelectedNode] = useState(null);
  const [searchResults, setSearchResults] = useState([]);
  const [algo, setAlgo] = useState('pca');
  const [queryPosition, setQueryPosition] = useState(null);

  const loadNodes = async () => {
    try {
      let data;
      if(algo === 'umap'){
        data = await vectorApi.getUMAPNodes();
      } else {
        data = await vectorApi.getPCANodes();
      }

      const nodesArray = Array.isArray(data) ? data : Object.values(data);
      const normalizedNodes = nodesArray.map(n => ({
        ...n,
        displayPos: n.reducedVector || [n.x, n.y, n.z || 0]
      }));
      setNodes(normalizedNodes);
    } catch (error) {
      console.error("Failed to fetch nodes:", error);
    }
  };


  useEffect(() => {
    loadNodes();
  }, [algo]);

  const handleSearch = async (queryText) => {
    try {
      const data = algo === 'umap'
        ? await vectorApi.searchUmap(queryText)
        : await vectorApi.search(queryText);

      const pos = data.queryPosition;
      console.log("Query position received:", pos);
      
      if (pos) {
        setQueryPosition(Array.isArray(pos) ? pos : [pos[0] ?? 0, pos[1] ?? 0, pos[2] ?? 0]);
      } else {
        console.error("API returned data but 'queryPosition' was missing");
      }

      if (data.results) {
        setSearchResults(data.results);
      }
    } catch (error) {
      console.error("Search failed:", error);
    }
  }

  const handleDocument = async (newDocument) => {
    try {
      await vectorApi.addDocument(newDocument);
      console.log("Added document:", newDocument.id);
      await loadNodes();
    } catch (error) {
      console.error("Failed to add document:", error);
    }
  }


  return (
    <div style={{ width: '100vw', height: '100vh', position: 'relative' }}>

      <SearchResults
        results={searchResults}
        onSelect={(hit) => {
          const node = nodes.find(n => n.id === hit.nodeId || n.documentId === hit.documentId);
          if (node) {
            setSelectedNode(node);
          } else {
            setSelectedNode({ 
              id: hit.documentId ?? `node:${hit.nodeId}`, 
              content: hit.document?.content ?? '' 
            });
          }
        }}
        onClose={() => setSearchResults([])}
      />

      {selectedNode && (
        <div className="node-info" style={infoBoxStyle}>
          <button onClick={() => setSelectedNode(null)} style={closeButtonStyle}>×</button>
          <h3>Node Details</h3>
          <p><strong>ID:</strong> {selectedNode.id}</p>
          <p><strong>Content:</strong> {selectedNode.content}</p>
        </div>
      )}

      <div className="controls" style={{
        position: 'absolute',
        zIndex: 1,
        top: 20,
        left: 20
      }}>
        <select value={algo} onChange={(e) => setAlgo(e.target.value)}>
          <option value="pca">PCA (Linear)</option>
          <option value="umap">UMAP (clusters)</option>
        </select>
        <select value={mode} onChange={(e) => setMode(e.target.value)}>
          <option value="standard">Standard</option>
          <option value="nebula">Nebula</option>
          <option value="blueprint">BluePrint</option>
        </select>
      </div>

      <div className="Search" style={{
        position: 'absolute',
        bottom: 20,
        left: 20,
        zIndex: 10
      }}>
        <PopoutSearch onSearch={handleSearch} />
        <PopoutMenu onSave={handleDocument} />
      </div>
      <Canvas camera={{ position: [10, 10, 10] }}>
        <Enviroment mode={mode} />

        {nodes.map((node) => (
          <VectorNode
            key={`${algo}-${node.id}`}
            node={node}
            position={node.displayPos}
            onSelect={setSelectedNode}
          />
        ))}

        {queryPosition && <QueryNode position={queryPosition} />}

        <OrbitControls makeDefault />
      </Canvas>
    </div>
  )
}

const infoBoxStyle = {
  position: 'absolute',
  top: '80px',
  right: '20px',
  background: 'rgba(25, 25, 25, 0.9)',
  backdropFilter: 'blur(8px)',
  border: '1px solid #3c3c3c',
  borderRadius: '10px',
  color: 'white',
  zIndex: 10,
  display: 'flex',
  flexDirection: 'column',
  boxShadow: '0 8px 32px rgba(0,0,0,0.5)',
  maxWidth: '300px',
  padding: '10px'
}

const closeButtonStyle = {
  position: 'absolute',
  background: 'none',
  fontFamily: 'sans-serif',
  top: '10px',
  right: '15px',
  padding: '0px',
  border: 'none',
  color: '#888',
  cursor: 'pointer',
  fontSize: '1.2rem'
};

